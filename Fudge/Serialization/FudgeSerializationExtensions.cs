/*<!--
 * Copyright (C) 2009 - 2010 by OpenGamma Inc. and other contributors.
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
using Fudge.Types;

namespace Fudge.Serialization
{
    /// <summary>
    /// Extension methods to simplify serialization code
    /// </summary>
    public static class FudgeSerializationExtensions
    {
        /// <summary>
        /// Serializes an object as an in-line submessage field with a given name.
        /// </summary>
        /// <param name="serializer">Serializer to write the field.</param>
        /// <param name="name">Name of field.</param>
        /// <param name="value">Object to serialize.</param>
        /// <remarks>
        /// By serializing the object as a sub-value, it will be written multiple times if referenced
        /// multple times in the object graph, it cannot be part of a cycle of references, and it does
        /// not support polymorphism.  If you need any of these features then use <see cref="WriteRef"/>
        /// instead.
        /// </remarks>
        public static void WriteInline(this IFudgeSerializer serializer, IMutableFudgeFieldContainer msg, string name, object value)
        {
            serializer.WriteInline(msg, name, null, value);
        }

        /// <summary>
        /// Writes a field only if the given value isn't null.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="serializer">Serializer to write the field.</param>
        /// <param name="name">Name of the field.</param>
        /// <param name="value">Value to write.</param>
        public static void WriteIfNotNull<T>(this IFudgeSerializer serializer, IMutableFudgeFieldContainer msg, string name, T value) where T : class
        {
            if (value != null)
                msg.Add(name, value);
        }

        /// <summary>
        /// Serializes all the values as a sequence of submessages with the same field name.
        /// </summary>
        /// <typeparam name="T">Type of objects.</typeparam>
        /// <param name="serializer">Serializer to write the fields.</param>
        /// <param name="name">Name of the fields.</param>
        /// <param name="objects">Objects to serialize.</param>
        /// <remarks>See the remarks in <see cref="WriteSubMsg"/> regarding the limitations of writing as an in-line sub-message.</remarks>
        /// <remarks><c>null</c>s in the sequence are written as Fudge <see cref="IndicatorType"/>s.</remarks>
        public static void WriteAllInline<T>(this IFudgeSerializer serializer, IMutableFudgeFieldContainer msg, string name, IEnumerable<T> objects)
        {
            serializer.WriteAllInline(msg, name, null, objects);
        }

        /// <summary>
        /// Serializes all the values as a sequence of submessages with the same field name and ordinal.
        /// </summary>
        /// <typeparam name="T">Type of objects.</typeparam>
        /// <param name="serializer">Serializer to write the fields.</param>
        /// <param name="name">Name of the fields.</param>
        /// <param name="ordinal">Ordinal of the fields (may be <c>null</c>).</param>
        /// <param name="objects">Objects to serialize.</param>
        /// <remarks>See the remarks in <see cref="WriteSubMsg"/> regarding the limitations of writing as an in-line sub-message.</remarks>
        /// <remarks><c>null</c>s in the sequence are written as Fudge <see cref="IndicatorType"/>s.</remarks>
        public static void WriteAllInline<T>(this IFudgeSerializer serializer, IMutableFudgeFieldContainer msg, string name, int? ordinal, IEnumerable<T> objects)
        {
            foreach (T obj in objects)
            {
                if (obj != null)
                {
                    serializer.WriteInline(msg, name, ordinal, obj);
                }
                else
                {
                    msg.Add(name, ordinal, IndicatorType.Instance);
                }
            }
        }
    }
}
