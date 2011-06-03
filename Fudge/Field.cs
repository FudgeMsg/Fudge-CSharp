/* <!--
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

namespace Fudge
{
    /// <summary>
    /// <c>Field</c> is a convenience class to allow functional construction of messages.
    /// </summary>
    /// <remarks>
    /// <c>Field</c> merely holds the data until they are added to a <see cref="FudgeMsg"/>, and
    /// in particular, the field type is not determined by <c>Field</c>.
    /// </remarks>
    /// <example>
    /// The following example shows constructing a message containing two sub-messages:
    /// <code>
    /// var inputMsg = new FudgeMsg(   
    ///                     new Field("sub1",
    ///                         new Field("bibble", "fibble"),
    ///                         new Field(827, "Blibble")),
    ///                     new Field("sub2", 
    ///                         new Field("bibble9", 9837438),
    ///                         new Field(828, 82.77f)));
    /// </code>
    /// </example>
    public class Field : IFudgeField
    {
        private readonly object value;
        private readonly short? ordinal;
        private readonly string name;

        /// <summary>
        /// Constructs a field with a name and value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public Field(string name, object value) : this(name, null, value)
        {
        }

        /// <summary>
        /// Constructs a field with an ordinal and value.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="value"></param>
        public Field(int ordinal, object value) : this(null, ordinal, value)
        {
        }

        /// <summary>
        /// Constructs a named field that contains a sub-message of other fields.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="subFields"></param>
        /// <example>
        /// The following example shows a hierarchical message being created using <see cref="Field"/>.
        /// <code>
        /// FudgeMsg inputMsg = context.NewMessage(
        ///            new Field("sub1",
        ///                new Field("bibble", "fibble"),
        ///                new Field(827, "Blibble")),
        ///            new Field("sub2",
        ///                new Field("bibble9", 9837438),
        ///                new Field(828, 82.77f)));
        /// 
        /// </code>
        /// </example>
        public Field(string name, params IFudgeField[] subFields) : this(name, null, new FieldContainer(subFields))
        {
        }

        /// <summary>
        /// Constructs a field with both a name and an ordinal plus value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ordinal"></param>
        /// <param name="value"></param>
        public Field(string name, int? ordinal, object value)
        {
            if (ordinal.HasValue && ordinal < short.MinValue || ordinal > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException("ordinal", "Ordinal must be within signed 16-bit range");
            }
            this.name = name;
            this.ordinal = (short?)ordinal;
            this.value = value;
        }

        #region IFudgeField Members

        /// <summary>
        /// Returns <c>null</c>.
        /// </summary>
        /// <remarks>The field type is not calculated until the field is added to a <see cref="FudgeMsg"/>.</remarks>
        public FudgeFieldType Type
        {
            get { return null; }
        }

        /// <inheritdoc/>
        public object Value
        {
            get { return value; }
        }

        /// <inheritdoc/>
        public short? Ordinal
        {
            get { return ordinal; }
        }

        /// <inheritdoc/>
        public string Name
        {
            get { return name ; }
        }

        #endregion

        /// <summary>
        /// Implementation of <see cref="IFudgeFieldContainer"/> purely to hold sub-fields
        /// </summary>
        private class FieldContainer : IFudgeFieldContainer
        {
            private readonly IFudgeField[] fields;

            public FieldContainer(IFudgeField[] fields)
            {
                this.fields = fields;
            }

            #region IFudgeFieldContainer Members

            public int GetNumFields()
            {
                return fields.Length;
            }

            public IList<IFudgeField> GetAllFields()
            {
                return fields;
            }

            public IList<string> GetAllFieldNames()
            {
                throw new NotSupportedException();
            }

            public IFudgeField GetByIndex(int index)
            {
                throw new NotSupportedException();
            }

            public IList<IFudgeField> GetAllByOrdinal(int ordinal)
            {
                throw new NotSupportedException();
            }

            public IFudgeField GetByOrdinal(int ordinal)
            {
                throw new NotSupportedException();
            }

            public IList<IFudgeField> GetAllByName(string name)
            {
                throw new NotSupportedException();
            }

            public IFudgeField GetByName(string name)
            {
                throw new NotSupportedException();
            }

            public object GetValue(string name)
            {
                throw new NotSupportedException();
            }

            public T GetValue<T>(string name)
            {
                throw new NotSupportedException();
            }

            public object GetValue(string name, Type type)
            {
                throw new NotSupportedException();
            }

            public object GetValue(int ordinal)
            {
                throw new NotSupportedException();
            }

            public T GetValue<T>(int ordinal)
            {
                throw new NotSupportedException();
            }

            public object GetValue(int ordinal, Type type)
            {
                throw new NotSupportedException();
            }

            public object GetValue(string name, int? ordinal)
            {
                throw new NotSupportedException();
            }

            public T GetValue<T>(string name, int? ordinal)
            {
                throw new NotSupportedException();
            }

            public object GetValue(string name, int? ordinal, Type type)
            {
                throw new NotSupportedException();
            }

            public double? GetDouble(string fieldName)
            {
                throw new NotSupportedException();
            }

            public double? GetDouble(int ordinal)
            {
                throw new NotSupportedException();
            }

            public float? GetFloat(string fieldName)
            {
                throw new NotSupportedException();
            }

            public float? GetFloat(int ordinal)
            {
                throw new NotSupportedException();
            }

            public long? GetLong(string fieldName)
            {
                throw new NotSupportedException();
            }

            public long? GetLong(int ordinal)
            {
                throw new NotSupportedException();
            }

            public int? GetInt(string fieldName)
            {
                throw new NotSupportedException();
            }

            public int? GetInt(int ordinal)
            {
                throw new NotSupportedException();
            }

            public short? GetShort(string fieldName)
            {
                throw new NotSupportedException();
            }

            public short? GetShort(int ordinal)
            {
                throw new NotSupportedException();
            }

            public sbyte? GetSByte(string fieldName)
            {
                throw new NotSupportedException();
            }

            public sbyte? GetSByte(int ordinal)
            {
                throw new NotSupportedException();
            }

            public bool? GetBoolean(string fieldName)
            {
                throw new NotSupportedException();
            }

            public bool? GetBoolean(int ordinal)
            {
                throw new NotSupportedException();
            }

            public string GetString(string fieldName)
            {
                throw new NotSupportedException();
            }

            public string GetString(int ordinal)
            {
                throw new NotSupportedException();
            }

            public IFudgeFieldContainer GetMessage(string fieldName)
            {
                throw new NotSupportedException();
            }

            public IFudgeFieldContainer GetMessage(int ordinal)
            {
                throw new NotSupportedException();
            }

            #endregion

            #region IEnumerable<IFudgeField> Members

            public IEnumerator<IFudgeField> GetEnumerator()
            {
                return ((IList<IFudgeField>)fields).GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return fields.GetEnumerator();
            }

            #endregion
        }

    }
}
