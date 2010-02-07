/* <!--
 * Copyright (C) 2009 - 2009 by OpenGamma Inc. and other contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *     
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * -->
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Fudge;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using IQToolkit;
using System.Threading;

namespace Fudge.Linq
{
    /// <summary>
    /// <c>FudgeLinqProvider</c> gives an implementation of <see cref="IQueryProvider"/> for
    /// sequences of <see cref="IFudgeFieldContainer"/>s.
    /// </summary>
    /// <remarks>
    /// <para>This is where the real work of Linq happens with <see cref="Expression"/>s that
    /// have been built on sequences of a reference type (by the compiler) being translated
    /// to operate on sequences of <see cref="IFudgeFieldContainer"/> instead.
    /// </para>
    /// <para>
    /// You would not normally construct one of these directly, but instead use the <c>AsQueryable</c>
    /// extension method on a <see cref="IEnumerable"/> of <see cref="IFudgeFieldContainer"/>s (e.g. a <c>List</c>
    /// or array).
    /// </para>
    /// <para>
    /// Note that we only currently support Select and Where clauses - this will be extended in
    /// the future.
    /// </para>
    /// <para>
    /// For a walkthrough of how this works, have a look at Matt Warren's MSDN blog at http://blogs.msdn.com/mattwar/pages/linq-links.aspx
    /// on which IQToolkit is based.
    /// </para>
    /// </remarks>
    public class FudgeLinqProvider : QueryProvider
    {
        private static readonly Dictionary<CacheEntry, CacheEntry> cache = new Dictionary<CacheEntry, CacheEntry>();
        private static readonly ReaderWriterLock cacheLock = new ReaderWriterLock();

        private readonly IEnumerable<IFudgeFieldContainer> source;
        private readonly bool useCache;

        /// <summary>
        /// Constructs a new <c>FudgeLinqProvider</c> from a set of <see cref="IFudgeFieldContainer"/>s (e.g. <see cref="FudgeMsg"/>s),
        /// using a cache to avoid recompilation of expressions we have already seen.
        /// </summary>
        /// <param name="source">Set of messages to operate on.</param>
        public FudgeLinqProvider(IEnumerable<IFudgeFieldContainer> source) : this(source, true)
        {
        }

        /// <summary>
        /// Constructs a new <c>FudgeLinqProvider</c> from a set of <see cref="IFudgeFieldContainer"/>s (e.g. <see cref="FudgeMsg"/>s),
        /// giving explicit control over whether to use a cache to avoid recompilation of expressions we have already seen.
        /// </summary>
        /// <param name="source">Set of messages to operate on.</param>
        /// <param name="useCache">If true then compiled expressions are cached and reused.</param>
        public FudgeLinqProvider(IEnumerable<IFudgeFieldContainer> source, bool useCache)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            this.source = source;
            this.useCache = useCache;
        }

        /// <summary>
        /// Gets the source of messages used by this provider.
        /// </summary>
        public IEnumerable<IFudgeFieldContainer> Source
        {
            get { return source; }
        }

        /// <inheritdoc/>
        public override string GetQueryText(Expression expression)
        {
            return expression.ToString();
        }

        /// <inheritdoc/>
        public override object Execute(Expression expression)
        {
            if (expression.NodeType != ExpressionType.Call)
                throw new Exception("Unsupported node type: " + expression.NodeType);

            var m = (MethodCallExpression)expression;
            if (m.Method.Name != "Select")
                throw new Exception("Unsupported method: " + m.Method.Name);

            // Pull out the constants from the new expression
            IList<object> values;
            var extractedLambda = ConstantExtractor.Extract(m, out values);

            // Now get compiled version from cache, or create as appropriate
            var cacheKey = new CacheEntry(m);
            var cachedEntry = GetCachedEntry(cacheKey);
            if (cachedEntry == null)
            {
                Type dataType = m.Method.GetGenericArguments()[0];                  // Queryable.Select<TSource,TResult>(...)

                // Perform the translation to using FudgeMsgs rather than the data type
                var translator = new FudgeExpressionTranslator(dataType, source);
                var newSelect = (LambdaExpression)translator.Translate(extractedLambda);

                // We can now create a fully-fledged cache entry
                cachedEntry = new CacheEntry(cacheKey, newSelect.Compile());
                AddCacheEntry(cachedEntry);
            }
            return cachedEntry.Invoke(values.ToArray());
        }        

        private CacheEntry GetCachedEntry(CacheEntry entry)
        {
            CacheEntry result = null;
            if (useCache)
            {
                cacheLock.AcquireReaderLock(Timeout.Infinite);

                cache.TryGetValue(entry, out result);

                cacheLock.ReleaseReaderLock();
            }
            return result;
        }

        private void AddCacheEntry(CacheEntry entry)
        {
            if (useCache)
            {
                cacheLock.AcquireWriterLock(Timeout.Infinite);

                cache[entry] = entry;

                cacheLock.ReleaseWriterLock();
            }
        }

        private class CacheEntry
        {
            private readonly int hashCode;
            private readonly Expression expression;
            private readonly Delegate compiledQuery;

            public CacheEntry(Expression expression)
            {
                this.expression = expression;
                this.hashCode = ExpressionTreeStructureHasher.ComputeHash(expression);
            }

            public CacheEntry(CacheEntry other, Delegate compiledQuery)
            {
                this.expression = other.expression;
                this.hashCode = other.hashCode;
                this.compiledQuery = compiledQuery;
            }

            public Expression Expression
            {
                get { return expression; }
            }

            public object Invoke(object[] args)
            {
                // TODO 2009-10-25 t0rx -- Could add some form of FastInvoke to speed this up
                return compiledQuery.DynamicInvoke(args);
            }

            public Delegate CompiledQuery
            {
                get { return compiledQuery; }
            }

            public override bool Equals(object obj)
            {
                if (obj == this)
                    return true;
                CacheEntry other = obj as CacheEntry;
                if (other == null)
                    return false;

                return ExpressionComparer.AreEqual(Expression, other.Expression, false);
            }

            public override int GetHashCode()
            {
                return hashCode;
            }
        }

        /// <summary>
        /// Extracts constants from the expression tree out so the compiled query can be used with
        /// different constants.
        /// </summary>
        /// <remarks>
        /// It'd be nice to use IQToolkit.QueryCache.QueryParameterizer, but that's not public
        /// </remarks>
        private class ConstantExtractor : ExpressionVisitor
        {
            private readonly List<ParameterExpression> newParameters = new List<ParameterExpression>();
            private readonly List<object> constantValues = new List<object>();

            public ConstantExtractor()
            {
            }

            public static LambdaExpression Extract(Expression exp, out IList<object> values)
            {
                var extractor = new ConstantExtractor();
                var body = extractor.Visit(exp);
                values = extractor.constantValues.ToArray();
                if (values.Count >= 5)
                {
                    // Can't create a lambda with more than 4 parameters.  Go figure.
                    values = new object[] { values };
                    return ParamArrayRewriter.Rewrite(body, extractor.newParameters.ToArray());
                }
                return Expression.Lambda(body, extractor.newParameters.ToArray());
            }

            protected override Expression VisitConstant(ConstantExpression c)
            {
                // Strip the constant out into a parameter
                var param = Expression.Parameter(c.Type, "c" + constantValues.Count);
                newParameters.Add(param);
                constantValues.Add(c.Value);
                if (c.Value is IQueryable)
                {
                    // We actually return c so it can be translated later
                    return c;
                }
                return param;
            }
        }

        /// <summary>
        /// This turns a body that expects a number of parameters, to instead have just one array param and index into that
        /// </summary>
        private class ParamArrayRewriter : ExpressionVisitor
        {
            private static readonly ParameterExpression paramParam = Expression.Parameter(typeof(object[]), "params");
            private readonly ParameterExpression[] parameters;

            public ParamArrayRewriter(ParameterExpression[] parameters)
            {
                this.parameters = parameters;
            }

            public static LambdaExpression Rewrite(Expression body, ParameterExpression[] parameters)
            {
                var rewriter = new ParamArrayRewriter(parameters);
                var newBody = rewriter.Visit(body);
                return Expression.Lambda(newBody, paramParam);
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (p == parameters[i])
                    {
                        // Rewrite it
                        return Expression.Convert(Expression.ArrayIndex(paramParam, Expression.Constant(i)), p.Type);
                    }
                }

                // Not found
                return p;
            }
        }
    }

}
