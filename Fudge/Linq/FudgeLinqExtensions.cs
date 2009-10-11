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
using IQToolkit;

namespace OpenGamma.Fudge.Linq
{
    public static class FudgeLinqExtensions
    {
        /// <summary>
        /// Map an <c>IEnumerable&lt;FudgeMsg&gt;</c> onto a <see cref="FudgeLinqQueryable"/> so
        /// that all the Linq compiler magic works.
        /// </summary>
        /// <typeparam name="T">Type of the object which has the structure of the message data.</typeparam>
        /// <param name="msgSource">Enumerable (array, list, etc.) to map</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>In the same way as Linq-to-SQL, Linq-to-Fudge relies on there being a proper type that the message
        /// can be mapped onto (i.e. a property with the same name and type as each field that we need to operate
        /// on in the message).  This is only used to make Intellisense and the compiler work - no actual
        /// deserialisation into objects happens whilst processing.
        /// </para>
        /// <para>See the <c>Linq.Examples</c> unit test for some examples.</para>
        /// </remarks>
        public static IQueryable<T> AsLinq<T>(this IEnumerable<FudgeMsg> msgSource)
        {
            return new Query<T>(new FudgeLinqProvider(msgSource));
        }
    }
}
