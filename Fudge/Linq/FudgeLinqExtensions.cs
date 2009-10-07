/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
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
