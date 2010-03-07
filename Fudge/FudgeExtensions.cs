/* <!--
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

namespace Fudge
{
    /// <summary>
    /// Extension methods for core Fudge types.
    /// </summary>
    public static class FudgeExtensions
    {
        #region IAppendingFudgeFieldContainer extensions

        /// <summary>
        /// Adds a field to a message, unless the value is null.
        /// </summary>
        /// <param name="msg">Message to contain the field.</param>
        /// <param name="name">Name of field to add.</param>
        /// <param name="value">Value to add.</param>
        public static void AddIfNotNull(this IAppendingFudgeFieldContainer msg, string name, object value)
        {
            if (value != null)
            {
                msg.Add(name, value);
            }
        }

        /// <summary>
        /// Adds a field to a message, unless the value is null.
        /// </summary>
        /// <param name="msg">Message to contain the field.</param>
        /// <param name="ordinal">Ordinal of field to add.</param>
        /// <param name="value">Value to add.</param>
        public static void AddIfNotNull(this IAppendingFudgeFieldContainer msg, int ordinal, object value)
        {
            if (value != null)
            {
                msg.Add(ordinal, value);
            }
        }

        /// <summary>
        /// Writes all the values as a sequence of fields with the same name and ordinal.
        /// </summary>
        /// <typeparam name="T">Type of values.</typeparam>
        /// <param name="msg">Message to write the fields.</param>
        /// <param name="name">Name of the fields.</param>
        /// <param name="values">Values to write.</param>
        /// <remarks><c>null</c>s in the sequence are written as Fudge <see cref="IndicatorType"/>s.</remarks>
        public static void AddAll<T>(this IAppendingFudgeFieldContainer msg, string name, IEnumerable<T> values)
        {
            AddAll(msg, name, null, values);
        }

        /// <summary>
        /// Writes all the values as a sequence of fields with the same name and ordinal.
        /// </summary>
        /// <typeparam name="T">Type of values.</typeparam>
        /// <param name="msg">Message to write the fields.</param>
        /// <param name="ordinal">Ordinal of the fields.</param>
        /// <param name="values">Values to write.</param>
        /// <remarks><c>null</c>s in the sequence are written as Fudge <see cref="IndicatorType"/>s.</remarks>
        public static void AddAll<T>(this IAppendingFudgeFieldContainer msg, int ordinal, IEnumerable<T> values)
        {
            AddAll(msg, null, ordinal, values);
        }

        /// <summary>
        /// Writes all the values as a sequence of fields with the same name and ordinal.
        /// </summary>
        /// <typeparam name="T">Type of values.</typeparam>
        /// <param name="msg">Message to write the fields.</param>
        /// <param name="name">Name of the fields.</param>
        /// <param name="ordinal">Ordinal of the fields (may be <c>null</c>).</param>
        /// <param name="values">Values to write.</param>
        /// <remarks><c>null</c>s in the sequence are written as Fudge <see cref="IndicatorType"/>s.</remarks>
        public static void AddAll<T>(this IAppendingFudgeFieldContainer msg, string name, int? ordinal, IEnumerable<T> values)
        {
            foreach (T value in values)
            {
                if (value != null)
                {
                    msg.Add(name, ordinal, value);
                }
                else
                {
                    msg.Add(name, ordinal, IndicatorType.Instance);
                }
            }
        }

        /// <summary>
        /// Adds all the fields in the enumerable to this message.
        /// </summary>
        /// <param name="fields">Enumerable of fields to add.</param>
        public static void AddAll(this IAppendingFudgeFieldContainer msg, IEnumerable<IFudgeField> fields)
        {
            if (fields == null)
                return; // Whatever

            foreach (var field in fields)
            {
                msg.Add(field);
            }
        }

        #endregion

        #region IFudgeField extensions

        /// <summary>
        /// Convenience method to get any field value as a string.
        /// </summary>
        /// <param name="field">Field containing the value.</param>
        /// <returns><c>null</c> if the field value is <c>null</c>, otherwise the result of calling <see cref="object.ToString"/> on the value.</returns>
        public static string GetString(this IFudgeField field)
        {
            object value = field.Value;
            if (value == null)
                return null;
            else
                return value.ToString();
        }

        #endregion
    }
}
