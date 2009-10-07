/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
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
    /// sequences of <see cref="FudgeMsg"/>s.
    /// </summary>
    /// <remarks>
    /// <para>This is where the real work of Linq happens with <see cref="Expression"/>s that
    /// have been built on sequences of a reference type (by the compiler) being translated
    /// to operate on sequences of <see cref="FudgeMsg"/> instead.
    /// </para>
    /// <para>
    /// You would not normally construct one of these directly, but instead use the <c>AsLinq</c>
    /// extension method on a <see cref="IEnumerable"/> of <see cref="FudgeMsg"/>s (e.g. a <c>List</c>
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
        private static readonly ParameterExpression msgParam = Expression.Parameter(typeof(FudgeMsg), "msg");
        
        private readonly IEnumerable<FudgeMsg> source;

        public FudgeLinqProvider(IEnumerable<FudgeMsg> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            this.source = source;
        }

        public IEnumerable<FudgeMsg> Source
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

            MethodCallExpression m = (MethodCallExpression)expression;
            if (m.Method.Name != "Select")
                throw new Exception("Unsupported method: " + m.Method.Name);

            Delegate newProjector = TranslateLambda(m.Arguments[1]).Compile();

            var readerSource = HandleSource(m.Arguments[0]);

            Type elementType = TypeHelper.GetElementType(expression.Type);
            Type readerType = typeof(FudgeLinqReader<>).MakeGenericType(elementType);
            return Activator.CreateInstance(readerType, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { readerSource, newProjector }, null);
        }

        private IEnumerable<FudgeMsg> HandleSource(Expression e)
        {
            switch (e.NodeType)
            {
                case ExpressionType.Constant:
                    // TODO t0rx 20091006 -- Check type
                    return source;
                case ExpressionType.Call:
                    var m = (MethodCallExpression)e;
                    var newSource = HandleSource(m.Arguments[0]);
                    switch (m.Method.Name)
                    {
                        case "Where":
                            var newLambda = TranslateLambda(m.Arguments[1]);
                            var newPredicate = (Func<FudgeMsg, bool>)(newLambda.Compile());
                            return newSource.Where(newPredicate);
                        // TODO t0rx 20091007 -- Handle OrderBy and other Linq methods
                        default:
                            throw new Exception("Unsupported method call :" + m.Method.Name);
                    }
            }
            return source;
        }

        private LambdaExpression TranslateLambda(Expression exp)
        {
            LambdaExpression oldLambda = (LambdaExpression)FudgeExpressionTranslator.StripQuotes(exp);
            Expression newBody = new FudgeExpressionTranslator(msgParam).Translate(oldLambda.Body);
            LambdaExpression newLambda = Expression.Lambda(newBody, msgParam);
            return newLambda;
        }

    }

}
