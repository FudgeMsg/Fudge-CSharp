/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge;
using System.Collections;
using System.Linq.Expressions;

namespace OpenGamma.Fudge.Linq
{
    /// <summary>
    /// <c>FudgeLinqQueryable&lt;T&gt;</c> provides an implementation of <see cref="IQueryable<T>"/>,
    /// essentially holding a Linq expression and provider.
    /// </summary>
    /// <typeparam name="T">Type of the result elements for the query.</typeparam>
    /// <remarks>
    /// This object is not usually constructed directly - use the <c>AsLinq</c> extension method to create
    /// the initial query, and Linq will obtain any further queries (for sub-clauses) from the provider.  For
    /// a walkthrough of how this works, have a look at Matt Warren's MSDN blog at http://blogs.msdn.com/mattwar/pages/linq-links.aspx
    /// </remarks>
    public class FudgeLinqQueryable<T> : IQueryable<T>
    {
        public FudgeLinqQueryable(FudgeLinqProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            Provider = provider;
            Expression = Expression.Constant(this);
        }
        
        public FudgeLinqQueryable(FudgeLinqProvider provider, Expression expression)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
                throw new ArgumentOutOfRangeException("expression");

            Provider = provider;
            Expression = expression;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Provider.Execute<IEnumerable>(Expression).GetEnumerator();
        }

        #endregion

        #region IQueryable Members

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public Expression Expression
        {
            get;
            private set;
        }

        public IQueryProvider Provider
        {
            get;
            private set;
        }

        #endregion
    }
}
