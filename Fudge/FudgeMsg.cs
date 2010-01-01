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
using System.Text;
using Fudge.Taxon;
using System.Diagnostics;
using System.IO;
using Fudge.Types;
using System.Collections;
using Fudge.Encodings;
using Fudge.Util;

namespace Fudge
{
    /// <summary>
    /// A container for <see cref="FudgeMsgField"/>s.
    /// This instance will contain all data fully extracted from a Fudge-encoded
    /// stream, unlike other systems where fields are unpacked as required.
    /// Therefore, constructing a <c>FudgeMsg</c> from a field is relatively more
    /// expensive in CPU and memory usage than just holding the original byte array,
    /// but lookups are substantially faster.
    /// </summary>
    /// <remarks>
    /// The various <c>Get*()</c> methods will return <c>null</c> if the field is not
    /// found, and otherwise use standard conversions to map between types, throwing
    /// <see cref="InvalidCastException"/> and <see cref="OverflowException"/> as
    /// appropriate.
    /// </remarks>
    public class FudgeMsg : FudgeEncodingObject, IMutableFudgeFieldContainer
    {
        private readonly FudgeContext fudgeContext;
        private readonly List<FudgeMsgField> fields = new List<FudgeMsgField>();

        /// <summary>
        /// Constructs a new <see cref="FudgeMsg"/> using a given <see cref="FudgeContext"/>.
        /// </summary>
        /// <param name="context">Context to use for the message.</param>
        public FudgeMsg(FudgeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context", "Context must be provided");
            }
            this.fudgeContext = context;
        }

        /// <summary>
        /// Creates a new <c>FudgeMsg</c> object as a copy of another.
        /// </summary>
        /// <param name="other">an existing <c>FudgeMsg</c> object to copy</param>
        public FudgeMsg(FudgeMsg other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("Cannot initialize from a null other FudgeMsg");
            }
            this.fudgeContext = other.fudgeContext;
            InitializeFromByteArray(other.ToByteArray());
        }

        /// <summary>
        /// Constructs a new <see cref="FudgeMsg"/> using a default context, and populates with a set of fields.
        /// </summary>
        /// <param name="fields">Fields to populate the message.</param>
        public FudgeMsg(params IFudgeField[] fields) : this(new FudgeContext(), fields)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="FudgeMsg"/> using a given context, and populates with a set of fields.
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> to use for the message.</param>
        /// <param name="fields">Fields to populate the message.</param>
        public FudgeMsg(FudgeContext context, params IFudgeField[] fields)
            : this(context)
        {
            foreach (var field in fields)
            {
                Add(field);
            }
        }

        /// <summary>
        /// Constructs a new <see cref="FudgeMsg"/> from raw binary data, using a given context.
        /// </summary>
        /// <param name="byteArray">Binary data to use.</param>
        /// <param name="context"><see cref="FudgeContext"/> for the message.</param>
        public FudgeMsg(byte[] byteArray, FudgeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context", "Context must be provided");
            }
        }

        /// <summary>
        /// Populates the message fields from the encoded data. If the array is larger than the Fudge envelope, any additional data is ignored.
        /// </summary>
        /// <param name="byteArray">the encoded data to populate this message with</param>
        protected void InitializeFromByteArray(byte[] byteArray)
        {
            FudgeMsgEnvelope other = fudgeContext.Deserialize(byteArray);
            fields.AddRange(other.Message.fields);
        }

        /// <summary>
        /// Gets the <see cref="FudgeContext"/> for this message.
        /// </summary>
        public FudgeContext FudgeContext
        {
            get { return fudgeContext; }
        }

        #region IMutableFudgeFieldContainer implementation

        /// <inheritdoc />
        public void Add(IFudgeField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("Cannot add an empty field");
            }
            Add(field.Name, field.Ordinal, field.Type, field.Value);
        }

        /// <inheritdoc />
        public void Add(string name, object value)
        {
            Add(name, null, null, value);
        }

        /// <inheritdoc />
        public void Add(int? ordinal, object value)
        {
            Add(null, ordinal, null, value);
        }

        /// <inheritdoc />
        public void Add(string name, int? ordinal, object value)
        {
            Add(name, ordinal, null, value);
        }

        /// <inheritdoc />
        public void Add(string name, int? ordinal, FudgeFieldType type, object value)
        {
            if (fields.Count >= short.MaxValue)
            {
                throw new InvalidOperationException("Can only add " + short.MaxValue + " to a single message.");
            }
            if (ordinal.HasValue && (ordinal < short.MinValue || ordinal > short.MaxValue))
            {
                throw new ArgumentOutOfRangeException("ordinal", "Ordinal must be within signed 16-bit range");
            }
            if (type == null)
            {
                // See if we can derive it
                type = fudgeContext.TypeHandler.DetermineTypeFromValue(value);
                if (type == null)
                {
                    throw new ArgumentException("Cannot determine a Fudge type for value " + value + " of type " + value.GetType());
                }
            }

            if (type == FudgeMsgFieldType.Instance && !(value is FudgeMsg))
            {
                // Copy the fields across to a new message
                value = CopyContainer((IFudgeFieldContainer)value);
            }

            // Adjust values to the lowest possible representation.
            value = type.Minimize(value, ref type);

            FudgeMsgField field = new FudgeMsgField(type, value, name, (short?)ordinal);
            fields.Add(field);
        }

        #endregion

        /// <summary>
        /// Adds all the fields in the enumerable to this message.
        /// </summary>
        /// <param name="fields">Enumerable of fields to add.</param>
        public void Add(IEnumerable<IFudgeField> fields)
        {
            // TODO t0rx 20091017 -- Add this method to IMutableFudgeFieldContainer?
            if (fields == null)
                return; // Whatever

            foreach (var field in fields)
            {
                Add(field);
            }
        }
        
        #region IFudgeFieldContainer implementation

        /// <inheritdoc />
        public short GetNumFields()
        {
            int size = fields.Count;
            Debug.Assert(size <= short.MaxValue);
            return (short)size;
        }

        /// <inheritdoc />
        public IList<IFudgeField> GetAllFields()
        {
            // Fudge-Java just returns a read-only wrapper, but we can't do that in a typed way in .net 3.5
            //var copy = new List<IFudgeField>(fields.Cast<IFudgeField>()); // Cast is specific to the linq namespace
            //return copy;
            return fields.ConvertAll<IFudgeField>(new Converter<FudgeMsgField, IFudgeField>(FudgeMsgField.toIFudgeField));
        }

        /// <inheritdoc />
        public IList<string> GetAllFieldNames()
        {
            // If only there was a set implementation...
            var dict = new Dictionary<string, bool>();
            foreach (var field in fields)
            {
                if (field.Name != null)
                {
                    dict[field.Name] = true;
                }
            }
            return new List<string>(dict.Keys);
        }

        /// <inheritdoc />
        public IFudgeField GetByIndex(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("Cannot specify a negative index into a FudgeMsg.");
            }
            if (index >= fields.Count)
            {
                return null;
            }
            return fields[index];
        }

        // REVIEW 2009-08-16 kirk -- All of these getters are currently extremely unoptimized.
        // there may be an option required if we have a lot of random access to the field content
        // to speed things up by building an index.
        /// <inheritdoc />
        public IList<IFudgeField> GetAllByOrdinal(int ordinal)
        {
            List<IFudgeField> result = new List<IFudgeField>();
            foreach (FudgeMsgField field in fields)
            {
                if (ordinal == field.Ordinal)
                {
                    result.Add(field);
                }
            }
            return result;
        }

        /// <inheritdoc />
        public IFudgeField GetByOrdinal(int ordinal)
        {
            foreach (FudgeMsgField field in fields)
            {
                if (ordinal == field.Ordinal)
                {
                    return field;
                }
            }
            return null;
        }

        /// <inheritdoc />
        public IList<IFudgeField> GetAllByName(string name)
        {
            List<IFudgeField> results = new List<IFudgeField>();
            foreach (FudgeMsgField field in fields)
            {
                if (name == field.Name)
                {
                    results.Add(field);
                }
            }
            return results;
        }

        /// <inheritdoc />
        public IFudgeField GetByName(string name)
        {
            foreach (FudgeMsgField field in fields)
            {
                if (name == field.Name)
                {
                    return field;
                }
            }
            return null;
        }

        /// <inheritdoc />
        public object GetValue(string name)
        {
            foreach (FudgeMsgField field in fields)
            {
                if (name == field.Name)
                {
                    return field.Value;
                }
            }
            return null;
        }

        /// <inheritdoc cref="IFudgeFieldContainer.GetValue{T}(System.String)" />
        public T GetValue<T>(string name)
        {
            return (T)GetValue(name, typeof(T));
        }

        /// <inheritdoc />
        public object GetValue(string name, Type type)
        {
            object value = GetValue(name);
            return fudgeContext.TypeHandler.ConvertType(value, type);
        }

        /// <inheritdoc />
        public object GetValue(int ordinal)
        {
            foreach (FudgeMsgField field in fields)
            {
                if (ordinal == field.Ordinal)
                {
                    return field.Value;
                }
            }
            return null;
        }

        /// <inheritdoc cref="IFudgeFieldContainer.GetValue{T}(System.Int32)" />
        public T GetValue<T>(int ordinal)
        {
            return (T)GetValue(ordinal, typeof(T));
        }

        /// <inheritdoc />
        public object GetValue(int ordinal, Type type)
        {
            object value = GetValue(ordinal);
            return fudgeContext.TypeHandler.ConvertType(value, type);
        }

        /// <inheritdoc />
        public object GetValue(string name, int? ordinal)
        {
            foreach (FudgeMsgField field in fields)
            {
                if ((ordinal != null) && (ordinal == field.Ordinal))
                {
                    return field.Value;
                }
                if ((name != null) && (name == field.Name))
                {
                    return field.Value;
                }
            }
            return null;
        }

        /// <inheritdoc cref="IFudgeFieldContainer.GetValue{T}(System.String,System.Int32?)"/>
        public T GetValue<T>(string name, int? ordinal)
        {
            return (T)GetValue(name, ordinal, typeof(T));
        }

        /// <inheritdoc />
        public object GetValue(string name, int? ordinal, Type type)
        {
            object value = GetValue(name, ordinal);
            return fudgeContext.TypeHandler.ConvertType(value, type);
        }

        // Primitive Queries:

        /// <inheritdoc />
        public double? GetDouble(string fieldName)
        {
            return GetAsDoubleInternal(fieldName, null);
        }

        /// <inheritdoc />
        public double? GetDouble(int ordinal)
        {
            return GetAsDoubleInternal(null, ordinal);
        }

        /// <summary>
        /// Internal implementation for GetDouble methods using the behaviour of <see cref="GetValue{T}(string,int?)" />.
        /// </summary>
        /// <param name="fieldName">field name, or null to search by ordinal only</param>
        /// <param name="ordinal">ordinal index, or null to search by field name only</param>
        /// <returns>field value cast as a double</returns>
        protected double? GetAsDoubleInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToDouble(null);
        }

        /// <inheritdoc />
        public float? GetFloat(string fieldName)
        {
            return GetAsFloatInternal(fieldName, null);
        }

        /// <inheritdoc />
        public float? GetFloat(int ordinal)
        {
            return GetAsFloatInternal(null, ordinal);
        }

        /// <summary>
        /// Internal implementation for GetFloat methods using the behaviour of <see cref="GetValue{T}(string,int?)" />.
        /// </summary>
        /// <param name="fieldName">field name, or null to search by ordinal only</param>
        /// <param name="ordinal">ordinal index, or null to search by field name only</param>
        /// <returns>field value cast as a float</returns>
        protected float? GetAsFloatInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToSingle(null);
        }

        /// <inheritdoc />
        public long? GetLong(string fieldName)
        {
            return GetAsLongInternal(fieldName, null);
        }

        /// <inheritdoc />
        public long? GetLong(int ordinal)
        {
            return GetAsLongInternal(null, ordinal);
        }

        /// <summary>
        /// Internal implementation for GetLong methods using the behaviour of <see cref="GetValue{T}(string,int?)" />.
        /// </summary>
        /// <param name="fieldName">field name, or null to search by ordinal only</param>
        /// <param name="ordinal">ordinal index, or null to search by field name only</param>
        /// <returns>field value cast as a long</returns>
        protected long? GetAsLongInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToInt64(null);
        }

        /// <inheritdoc />
        public int? GetInt(string fieldName)
        {
            return GetAsIntInternal(fieldName, null);
        }

        /// <inheritdoc />
        public int? GetInt(int ordinal)
        {
            return GetAsIntInternal(null, ordinal);
        }

        /// <summary>
        /// Internal implementation for GetIntmethods using the behaviour of <see cref="GetValue{T}(string,int?)" />.
        /// </summary>
        /// <param name="fieldName">field name, or null to search by ordinal only</param>
        /// <param name="ordinal">ordinal index, or null to search by field name only</param>
        /// <returns>field value cast as an int</returns>
        protected int? GetAsIntInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToInt32(null);
        }

        /// <inheritdoc />
        public short? GetShort(string fieldName)
        {
            return GetAsShortInternal(fieldName, null);
        }

        /// <inheritdoc />
        public short? GetShort(int ordinal)
        {
            return GetAsShortInternal(null, ordinal);
        }

        /// <summary>
        /// Internal implementation for GetShort methods using the behaviour of <see cref="GetValue{T}(string,int?)" />.
        /// </summary>
        /// <param name="fieldName">field name, or null to search by ordinal only</param>
        /// <param name="ordinal">ordinal index, or null to search by field name only</param>
        /// <returns>field value cast as a short</returns>
        protected short? GetAsShortInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToInt16(null);
        }

        /// <inheritdoc />
        public sbyte? GetSByte(string fieldName)
        {
            return GetAsSByteInternal(fieldName, null);
        }

        /// <inheritdoc />
        public sbyte? GetSByte(int ordinal)
        {
            return GetAsSByteInternal(null, ordinal);
        }

        /// <summary>
        /// Internal implementation for GetSByte methods using the behaviour of <see cref="GetValue{T}(string,int?)" />.
        /// </summary>
        /// <param name="fieldName">field name, or null to search by ordinal only</param>
        /// <param name="ordinal">ordinal index, or null to search by field name only</param>
        /// <returns>field value cast as a sbyte</returns>
        protected sbyte? GetAsSByteInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToSByte(null);
        }

        /// <inheritdoc />
        public bool? GetBoolean(string fieldName)
        {
            return GetAsBooleanInternal(fieldName, null);
        }

        /// <inheritdoc />
        public bool? GetBoolean(int ordinal)
        {
            return GetAsBooleanInternal(null, ordinal);
        }

        /// <summary>
        /// Internal implementation for GetBoolean methods using the behaviour of <see cref="GetValue{T}(string,int?)" />.
        /// </summary>
        /// <param name="fieldName">field name, or null to search by ordinal only</param>
        /// <param name="ordinal">ordinal index, or null to search by field name only</param>
        /// <returns>field value cast as a boolean</returns>
        protected bool? GetAsBooleanInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToBoolean(null);
        }

        /// <inheritdoc />
        public string GetString(string fieldName)
        {
            return GetAsStringInternal(fieldName, null);
        }

        /// <inheritdoc />
        public string GetString(int ordinal)
        {
            return GetAsStringInternal(null, ordinal);
        }

        /// <inheritdoc />
        public IFudgeFieldContainer GetMessage(string name)
        {
            return (IFudgeFieldContainer)GetFirstTypedValue(name, FudgeTypeDictionary.FUDGE_MSG_TYPE_ID);
        }

        /// <inheritdoc />
        public IFudgeFieldContainer GetMessage(int ordinal)
        {
            return (IFudgeFieldContainer)GetFirstTypedValue(ordinal, FudgeTypeDictionary.FUDGE_MSG_TYPE_ID);
        }

        /// <summary>
        /// Internal implementation for GetString methods using the behaviour of <see cref="GetValue{T}(string,int?)" />.
        /// </summary>
        /// <param name="fieldName">field name, or null to search by ordinal only</param>
        /// <param name="ordinal">ordinal index, or null to search by field name only</param>
        /// <returns>field value cast as a string</returns>
        protected string GetAsStringInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToString(null);
        }

        #endregion

        private FudgeMsg CopyContainer(IFudgeFieldContainer container)
        {
            var msg = fudgeContext.NewMessage();
            msg.Add(container);
            return msg;
        }

        /// <summary>
        /// Returns the Fudge encoded form of this <c>FudgeMsg</c> as a <c>byte</c> array without a taxonomy reference.
        /// </summary>
        /// <returns>an array containing the encoded message</returns>
        public byte[] ToByteArray()
        {
            return fudgeContext.ToByteArray(this);
        }

        // TODO 2009-12-14 Andrew -- should we have a ToByteArray that accepts a taxonomy ?

        /// <inheritdoc />
        public override int ComputeSize(IFudgeTaxonomy taxonomy)
        {
            int size = 0;
            foreach (FudgeMsgField field in fields)
            {
                size += field.GetSize(taxonomy);
            }
            return size;
        }

        /// <summary>
        /// Returns the value of the first field with a given name and type identifier.
        /// </summary>
        /// <param name="fieldName">field name</param>
        /// <param name="typeId">type identifier</param>
        /// <returns>the matching field, or null if none is found</returns>
        protected object GetFirstTypedValue(string fieldName, int typeId)
        {
            foreach (FudgeMsgField field in fields)
            {
                if ((fieldName == field.Name)
                    && (field.Type.TypeId == typeId))
                {
                    return field.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the value of the first field with a given ordinal index and type identifier.
        /// </summary>
        /// <param name="ordinal">ordinal index</param>
        /// <param name="typeId">type identifier</param>
        /// <returns>matching field, or null if none found</returns>
        protected object GetFirstTypedValue(int ordinal, int typeId)
        {
            foreach (FudgeMsgField field in fields)
            {
                if (field.Ordinal == null)
                    continue;

                if ((field.Ordinal == ordinal)
                  && (field.Type.TypeId == typeId))
                {
                    return field.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Updates any fields which have an ordinal index only to include a field name if one is available in the taxonomy. The
        /// same taxonomy is passed to any submessages.
        /// </summary>
        /// <param name="taxonomy">taxonomy to set field names from</param>
        public void SetNamesFromTaxonomy(IFudgeTaxonomy taxonomy)
        {
            if (taxonomy == null)
            {
                return;
            }
            for (int i = 0; i < fields.Count; i++)
            {
                FudgeMsgField field = fields[i];
                if ((field.Ordinal != null) && (field.Name == null))
                {
                    string nameFromTaxonomy = taxonomy.GetFieldName(field.Ordinal.Value);
                    if (nameFromTaxonomy == null)
                    {
                        continue;
                    }
                    FudgeMsgField replacementField = new FudgeMsgField(field.Type, field.Value, nameFromTaxonomy, field.Ordinal);
                    fields[i] = replacementField;
                }

                if (field.Value is FudgeMsg)
                {
                    FudgeMsg subMsg = (FudgeMsg)field.Value;
                    subMsg.SetNamesFromTaxonomy(taxonomy);
                }
            }
        }

        #region IEnumerable<IFudgeField> Members

        /// <inheritdoc />
        public IEnumerator<IFudgeField> GetEnumerator()
        {
            var copy = new List<FudgeMsgField>(fields);
            foreach (object field in copy)
            {
                yield return (IFudgeField)field;
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            var copy = new List<FudgeMsgField>(fields);
            return copy.GetEnumerator();
        }


        #endregion

        /// <summary>
        /// Returns a string representation of this message.
        /// </summary>
        /// <returns>string representation</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("FudgeMsg[");
            foreach (var field in this)
            {
                if (field.Ordinal != null)
                {
                    sb.Append(field.Ordinal);
                    sb.Append(": ");
                }
                if (field.Name != null)
                {
                    sb.Append(field.Name);
                }
                sb.Append(" => ");
                sb.Append(field.Value);
                sb.Append(", ");
            }
            if (sb.Length > 13)
            {
                sb.Remove(sb.Length - 2, 2);
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
