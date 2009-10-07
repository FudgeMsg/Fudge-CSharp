/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using OpenGamma.Fudge;

namespace OpenGamma.Fudge.Linq
{
    /// <summary>
    /// The <c>FudgeLinqReader</c> class provides a lazy projection of the source IEnumerable&lt;FudgeMsg&gt;
    /// onto the results of the <c>Select</c> clause.
    /// </summary>
    /// <typeparam name="T">Type of the result of the <c>Select clause</c></typeparam>
    /// <remarks>Based on Matt Warren's MSDN Blog at http://blogs.msdn.com/mattwar/archive/2007/08/02/linq-building-an-iqueryable-provider-part-iv.aspx</remarks>
    internal class FudgeLinqReader<T> : IEnumerable<T>
    {
        private Enumerator enumerator;

        internal FudgeLinqReader(IEnumerable<FudgeMsg> source, Func<FudgeMsg, T> projector)
        {
            enumerator = new Enumerator(source.GetEnumerator(), projector);
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            if (enumerator == null)
                throw new InvalidOperationException("Cannot call GetEnumerator twice.");

            var result = enumerator;
            enumerator = null;
            return result;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        private class Enumerator : IEnumerator<T>
        {
            private readonly IEnumerator<FudgeMsg> source;
            private readonly Func<FudgeMsg, T> projector;
            private T current;

            public Enumerator(IEnumerator<FudgeMsg> source, Func<FudgeMsg, T> projector)
            {
                this.source = source;
                this.projector = projector;
            }

            #region IEnumerator<T> Members

            public T Current
            {
                get { return current; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get { return current; }
            }

            public bool MoveNext()
            {
                if (source.MoveNext())
                {
                    current = projector(source.Current);
                    return true;
                }
                return false;
            }

            public void Reset()
            {
            }

            #endregion
        }
    }
}
