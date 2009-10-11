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
using System.Collections;
using OpenGamma.Fudge;

namespace OpenGamma.Fudge.Linq
{
    /// <summary>
    /// The <c>FudgeLinqReader</c> class provides a lazy projection of the source IEnumerable&lt;FudgeMsg&gt;
    /// onto the results of the <c>Select</c> clause.
    /// </summary>
    /// <typeparam name="T">Type of the result of the <c>Select clause</c></typeparam>
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
