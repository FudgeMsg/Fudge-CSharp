/*
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
using Fudge.Taxon;
using System.Diagnostics;
using System.IO;
using Fudge.Types;
using System.Collections;
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
        private readonly FudgeTypeDictionary typeDictionary;
        private readonly List<FudgeMsgField> fields = new List<FudgeMsgField>();

        public FudgeMsg() : this(FudgeTypeDictionary.Instance)
        {
        }

        public FudgeMsg(FudgeTypeDictionary typeDictionary)
        {
            if (typeDictionary == null)
            {
                throw new ArgumentNullException("typeDictionary", "Type dictionary must be provided");
            }
            this.typeDictionary = typeDictionary;
        }

        public FudgeMsg(FudgeMsg other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("Cannot initialize from a null other FudgeMsg");
            }
            InitializeFromByteArray(other.ToByteArray());
            this.typeDictionary = other.typeDictionary;
        }

        public FudgeMsg(params IFudgeField[] fields) : this(FudgeTypeDictionary.Instance)
        {
            foreach (var field in fields)
            {
                Add(field);
            }
        }

        public FudgeMsg(byte[] byteArray, FudgeTypeDictionary typeDictionary, IFudgeTaxonomy taxonomy)
        {
            if (typeDictionary == null)
            {
                throw new ArgumentNullException("typeDictionary", "Type dictionary must be provided");
            }
            this.typeDictionary = typeDictionary;
            InitializeFromByteArray(byteArray);
        }

        protected void InitializeFromByteArray(byte[] byteArray)
        {
            MemoryStream stream = new MemoryStream(byteArray);
            FudgeBinaryReader bw = new FudgeBinaryReader(stream);
            FudgeMsgEnvelope other;
            try
            {
                other = FudgeStreamDecoder.ReadMsg(bw);
            }
            catch (IOException e)
            {
                throw new FudgeRuntimeException("IOException thrown using BinaryReader", e);      // TODO t0rx 2009-08-31 -- This is just RuntimeException in Fudge-Java
            }
            fields.AddRange(other.Message.fields);
        }

        public FudgeTypeDictionary TypeDictionary
        {
            get { return typeDictionary; }
        }

        #region IMutableFudgeFieldContainer implementation
        public void Add(IFudgeField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("Cannot add an empty field");
            }
            Add(field.Name, field.Ordinal, field.Type, field.Value);
        }

        public void Add(string name, object value)
        {
            Add(name, null, value);
        }

        public void Add(int? ordinal, object value)
        {
            Add(null, ordinal, value);
        }

        public void Add(string name, int? ordinal, object value)
        {
            FudgeFieldType type = DetermineTypeFromValue(value, typeDictionary);
            if (type == null)
            {
                throw new ArgumentException("Cannot determine a Fudge type for value " + value + " of type " + value.GetType());
            }
            Add(name, ordinal, type, value);
        }

        public virtual void Add(string name, int? ordinal, FudgeFieldType type, object value)
        {
            if (fields.Count >= short.MaxValue)
            {
                throw new InvalidOperationException("Can only add " + short.MaxValue + " to a single message.");
            }
            if (type == null)
            {
                throw new ArgumentNullException("Cannot add a field without a type specified.");
            }
            if (ordinal.HasValue && (ordinal < short.MinValue || ordinal > short.MaxValue))
            {
                throw new ArgumentOutOfRangeException("ordinal", "Ordinal must be within signed 16-bit range");
            }

            // Adjust values to the lowest possible representation.
            value = type.Minimize(value, ref type);

            FudgeMsgField field = new FudgeMsgField(type, value, name, (short?)ordinal);
            fields.Add(field);
        }

        protected internal static FudgeFieldType DetermineTypeFromValue(object value, FudgeTypeDictionary typeDictionary)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Cannot determine type for null value.");
            }
            FudgeFieldType type = typeDictionary.GetByCSharpType(value.GetType());
            if ((type == null) && (value is UnknownFudgeFieldValue))
            {
                UnknownFudgeFieldValue unknownValue = (UnknownFudgeFieldValue)value;
                type = unknownValue.Type;
            }
            return type;
        }

        #endregion

        public void Add(IEnumerable<IFudgeField> fields)
        {
            // TODO t0rx 20091017 -- Add this method to IMutableFudgeFieldContainer?
            if (fields == null)
                return;             // Whatever

            foreach (var field in fields)
            {
                Add(field);
            }
        }

        public void AddAll<T>(string name, IEnumerable<T> values)
        {
            foreach (T val in values)
            {
                Add(name, val);
            }
        }

        public void AddAll<T>(int ordinal, IEnumerable<T> values)
        {
            foreach (T val in values)
            {
                Add(ordinal, val);
            }
        }

        #region IFudgeFieldContainer implementation
        public short GetNumFields()
        {
            int size = fields.Count;
            Debug.Assert(size <= short.MaxValue);
            return (short)size;
        }

        /// <summary>
        /// Return an unmodifiable list of all the fields in this message, in the index
        /// order for those fields.
        /// </summary>
        /// <returns></returns>
        public IList<IFudgeField> GetAllFields()
        {
            // Fudge-Java just returns a read-only wrapper, but we can't do that in a typed way in .net 3.5
            var copy = new List<IFudgeField>(fields.Cast<IFudgeField>());
            return copy;
        }

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

        // REVIEW kirk 2009-08-16 -- All of these getters are currently extremely unoptimized.
        // there may be an option required if we have a lot of random access to the field content
        // to speed things up by building an index.
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

        public virtual object GetValue(string name)
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

        public IList<T> GetAllValues<T>(string name)
        {
            var fields = GetAllByName(name);
            int nFields = fields.Count;
            T[] result = new T[nFields];
            for (int i = 0; i < nFields; i++)
            {
                result[i] = (T)ConvertType(fields[i].Value, typeof(T));
            }
            return result;
        }

        public IList<T> GetAllValues<T>(int ordinal)
        {
            var fields = GetAllByOrdinal(ordinal);
            int nFields = fields.Count;
            T[] result = new T[nFields];
            for (int i = 0; i < nFields; i++)
            {
                result[i] = (T)ConvertType(fields[i].Value, typeof(T));
            }
            return result;
        }

        public T GetValue<T>(string name)
        {
            return (T)GetValue(name, typeof(T));
        }

        public object GetValue(string name, Type type)
        {
            object value = GetValue(name);
            return ConvertType(value, type);
        }

        public virtual object GetValue(int ordinal)
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

        public T GetValue<T>(int ordinal)
        {
            return (T)GetValue(ordinal, typeof(T));
        }

        public object GetValue(int ordinal, Type type)
        {
            object value = GetValue(ordinal);
            return ConvertType(value, type);
        }

        public virtual object GetValue(string name, int? ordinal)
        {
            int index = GetIndex(name, ordinal);
            return index == -1 ? null : fields[index].Value;
        }

        protected int GetIndex(string name, int? ordinal)
        {
            int nFields = fields.Count;
            for (int i = 0; i < nFields; i++)
            {
                var field = fields[i];
                if ((ordinal != null) && (ordinal == field.Ordinal))
                {
                    return i;
                }
                if ((name != null) && (name == field.Name))
                {
                    return i;
                }
            }

            // Not found
            return -1;
        }

        public T GetValue<T>(string name, int? ordinal)
        {
            return (T)GetValue(name, ordinal, typeof(T));
        }

        public object GetValue(string name, int? ordinal, Type type)
        {
            object value = GetValue(name, ordinal);
            return ConvertType(value, type);
        }

        // Primitive Queries:

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>double</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>double</c></exception>
        public double? GetDouble(string fieldName)
        {
            return GetAsDoubleInternal(fieldName, null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>double</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>double</c></exception>
        public double? GetDouble(int ordinal)
        {
            return GetAsDoubleInternal(null, ordinal);
        }

        protected double? GetAsDoubleInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToDouble(null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>float</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>float</c></exception>
        public float? GetFloat(string fieldName)
        {
            return GetAsFloatInternal(fieldName, null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>float</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>float</c></exception>
        public float? GetFloat(int ordinal)
        {
            return GetAsFloatInternal(null, ordinal);
        }

        protected float? GetAsFloatInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToSingle(null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>long</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>long</c></exception>
        public long? GetLong(string fieldName)
        {
            return GetAsLongInternal(fieldName, null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>long</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>long</c></exception>
        public long? GetLong(int ordinal)
        {
            return GetAsLongInternal(null, ordinal);
        }

        protected long? GetAsLongInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToInt64(null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to an <c>int</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within an <c>int</c></exception>
        public int? GetInt(string fieldName)
        {
            return GetAsIntInternal(fieldName, null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to an <c>int</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within an <c>int</c></exception>
        public int? GetInt(int ordinal)
        {
            return GetAsIntInternal(null, ordinal);
        }

        protected int? GetAsIntInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToInt32(null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>short</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>short</c></exception>
        public short? GetShort(string fieldName)
        {
            return GetAsShortInternal(fieldName, null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>short</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>short</c></exception>
        public short? GetShort(int ordinal)
        {
            return GetAsShortInternal(null, ordinal);
        }

        protected short? GetAsShortInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToInt16(null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>byte</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>byte</c></exception>
        public sbyte? GetSByte(string fieldName)
        {
            return GetAsSByteInternal(fieldName, null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>byte</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>byte</c></exception>
        public sbyte? GetSByte(int ordinal)
        {
            return GetAsSByteInternal(null, ordinal);
        }

        protected sbyte? GetAsSByteInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToSByte(null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>bool</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>bool</c></exception>
        public bool? GetBoolean(string fieldName)
        {
            return GetAsBooleanInternal(fieldName, null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>bool</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>bool</c></exception>
        public bool? GetBoolean(int ordinal)
        {
            return GetAsBooleanInternal(null, ordinal);
        }

        protected bool? GetAsBooleanInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToBoolean(null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>string</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>string</c></exception>
        public string GetString(string fieldName)
        {
            return GetAsStringInternal(fieldName, null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>string</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>string</c></exception>
        public string GetString(int ordinal)
        {
            return GetAsStringInternal(null, ordinal);
        }

        public IFudgeFieldContainer GetMessage(string name)
        {
            return (IFudgeFieldContainer)GetFirstTypedValue(name, FudgeTypeDictionary.FUDGE_MSG_TYPE_ID);
        }

        public IFudgeFieldContainer GetMessage(int ordinal)
        {
            return (IFudgeFieldContainer)GetFirstTypedValue(ordinal, FudgeTypeDictionary.FUDGE_MSG_TYPE_ID);
        }

        protected string GetAsStringInternal(string fieldName, int? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToString(null);
        }

        public byte[] ToByteArray()
        {
            MemoryStream stream = new MemoryStream(ComputeSize(null));
            FudgeBinaryWriter bw = new FudgeBinaryWriter(stream);
            try
            {
                FudgeStreamEncoder.WriteMsg(bw, this);
                bw.Flush();
            }
            catch (IOException e)
            {
                throw new FudgeRuntimeException("Had an IOException writing to a MemoryStream.", e);        // TODO t0rx 2009-08-30 -- In Fudge-Java this is just a RuntimeException
            }

            return stream.ToArray();
        }
        #endregion

        public override int ComputeSize(IFudgeTaxonomy taxonomy)
        {
            int size = 0;
            foreach (FudgeMsgField field in fields)
            {
                size += field.GetSize(taxonomy);
            }
            return size;
        }

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

        private object ConvertType(object value, Type type)
        {
            if (value == null) return null;

            if (!type.IsAssignableFrom(value.GetType()))
            {
                FudgeFieldType fieldType = TypeDictionary.GetByCSharpType(type);
                if (fieldType == null)
                    throw new InvalidCastException("No registered field type for " + type.Name);

                value = fieldType.ConvertValueFrom(value);
            }
            return value;
        }

        #region IEnumerable<IFudgeField> Members

        public IEnumerator<IFudgeField> GetEnumerator()
        {
            var copy = new List<FudgeMsgField>(fields);
            foreach (object field in copy)
            {
                yield return (IFudgeField)field;
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            var copy = new List<FudgeMsgField>(fields);
            return copy.GetEnumerator();
        }

        #endregion

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
