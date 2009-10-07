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
using System.Reflection;

namespace OpenGamma.Fudge.Linq
{
    /// <summary>
    /// Used to translate <see cref="Expression"/>s so that calls to get values from members of the
    /// reference type become <c>GetValue</c> calls on the <see cref="FudgeMsg"/> instead.
    /// </summary>
    internal class FudgeExpressionTranslator : ExpressionVisitor
    {
        private static MethodInfo getValueMethod = typeof(FudgeMsg).GetMethod("GetValue", new Type[] { typeof(string), typeof(Type) });
        private readonly ParameterExpression msgParam;

        public FudgeExpressionTranslator(ParameterExpression msgParam)
        {
            this.msgParam = msgParam;
        }

        public Expression Translate(Expression exp)
        {
            return this.Visit(exp);
        }

        public static Expression StripQuotes(Expression exp)
        {
            while (exp.NodeType == ExpressionType.Quote)
                exp = ((UnaryExpression)exp).Operand;
            return exp;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable))
            {
                switch (m.Method.Name)
                {
                    case "Select":
                    case "Where":
                        LambdaExpression oldLambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                        Expression newBody = Visit(oldLambda.Body);
                        LambdaExpression newLambda = Expression.Lambda(newBody, msgParam);
                        return Expression.Call(newLambda, m.Method);
                    default:
                        throw new Exception("Unsupported method call: " + m.Method.Name);
                }
            }
            return base.VisitMethodCall(m);
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                // Change the member access to the data type into a call to get the value from the message
                return Expression.Convert(Expression.Call(msgParam, getValueMethod, Expression.Constant(m.Member.Name), Expression.Constant(m.Type)), m.Type);
            }
            else
            {
                return base.VisitMemberAccess(m);
            }
        }
    }
}
