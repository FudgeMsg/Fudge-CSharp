/**
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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using OpenGamma.Fudge;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using IQToolkit;

namespace OpenGamma.Fudge.Linq
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
        private static readonly ParameterExpression msgParam = Expression.Parameter(typeof(IFudgeFieldContainer), "msg");

        private readonly IEnumerable<IFudgeFieldContainer> source;

        public FudgeLinqProvider(IEnumerable<IFudgeFieldContainer> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            this.source = source;
        }

        public IEnumerable<IFudgeFieldContainer> Source
        {
            get { return source; }
        }

        public override string GetQueryText(Expression expression)
        {
            return expression.ToString();
        }

        public override object Execute(Expression expression)
        {
            if (expression.NodeType != ExpressionType.Call)
                throw new Exception("Unsupported node type: " + expression.NodeType);

            var m = (MethodCallExpression)expression;
            if (m.Method.Name != "Select")
                throw new Exception("Unsupported method: " + m.Method.Name);

            Type dataType = m.Method.GetGenericArguments()[0];                  // Queryable.Select<TSource,TResult>(...)

            var translator = new FudgeExpressionTranslator(dataType, msgParam, source);

            // Translate our select, and pull out the resulting IEnumerable<IFudgeFieldContainer> and projection function that our reader needs
            var newSelect = (MethodCallExpression)translator.Translate(m);
            var lhsValue = Expression.Lambda(newSelect.Arguments[0]).Compile().DynamicInvoke();
            Delegate projector = ((LambdaExpression)(newSelect.Arguments[1])).Compile();

            // Construct a reader of the right type
            Type elementType = TypeHelper.GetElementType(expression.Type);
            Type readerType = typeof(FudgeLinqReader<>).MakeGenericType(elementType);
            return Activator.CreateInstance(readerType, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { lhsValue, projector }, null);
        }
    }

}
