/**
* Copyright (C) 2009 - 2009 by OpenGamma Inc.
*
* Please see distribution for license.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge.Taxon;
using System.Diagnostics;
using System.IO;
using OpenGamma.Fudge.Types;

namespace OpenGamma.Fudge
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
    /// <see cref="InvalidCastException"/> and <see cref="OverFlowException"/> as
    /// appropriate.
    /// </remarks>
    public class FudgeMsg : FudgeEncodingObject
    {
        // TODO t0rx 2009-08-30 -- Finish porting FudgeMsg

        private readonly List<FudgeMsgField> fields = new List<FudgeMsgField>();

        public FudgeMsg()
        {
        }

        public FudgeMsg(FudgeMsg other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("Cannot initialize from a null other FudgeMsg");
            }
            InitializeFromByteArray(other.ToByteArray());
        }

        public FudgeMsg(byte[] byteArray, IFudgeTaxonomy taxonomy)
        {
            InitializeFromByteArray(byteArray);
        }

        protected void InitializeFromByteArray(byte[] byteArray)
        {
            MemoryStream stream = new MemoryStream(byteArray);
            BinaryReader bw = new BinaryReader(stream);
            FudgeMsg other;
            try
            {
                other = FudgeStreamDecoder.ReadMsg(bw);
            }
            catch (IOException e)
            {
                throw new FudgeRuntimeException("IOException thrown using BinaryReader", e);      // TODO t0rx 2009-08-31 -- This is just RuntimeException in Fudge-Java
            }
            fields.AddRange(other.fields);
        }

        public void Add(IFudgeField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("Cannot add an empty field");
            }
            fields.Add(new FudgeMsgField(field));
        }

        public void Add(object value, string name)
        {
            Add(value, name, null);
        }

        public void Add(object value, short? ordinal)
        {
            Add(value, null, ordinal);
        }

        public void Add(object value, string name, short? ordinal)
        {
            FudgeFieldType type = DetermineTypeFromValue(value);
            if (type == null)
            {
                throw new ArgumentException("Cannot determine a Fudge type for value " + value + " of type " + value.GetType());
            }
            Add(type, value, name, ordinal);
        }

        public void Add(FudgeFieldType type, object value, string name, short? ordinal)
        {
            if (fields.Count >= short.MaxValue)
            {
                throw new InvalidOperationException("Can only add " + short.MaxValue + " to a single message.");
            }
            if (type == null)
            {
                throw new ArgumentNullException("Cannot add a field without a type specified.");
            }

            // Adjust integral values to the lowest possible representation.
            switch (type.TypeId)
            {
                case FudgeTypeDictionary.BOOLEAN_TYPE_ID:
                    if (!(bool)value)
                    {
                        value = IndicatorType.Instance;
                        type = IndicatorFieldType.Instance;
                    }
                    break;
                case FudgeTypeDictionary.BYTE_TYPE_ID:
                case FudgeTypeDictionary.SHORT_TYPE_ID:
                case FudgeTypeDictionary.INT_TYPE_ID:
                case FudgeTypeDictionary.LONG_TYPE_ID:
                    long valueAsLong = System.Convert.ToInt64(value);
                    if (valueAsLong == 0)
                    {
                        value = IndicatorType.Instance;
                        type = IndicatorFieldType.Instance;
                    }
                    else if ((valueAsLong >= byte.MinValue) && (valueAsLong <= byte.MaxValue))
                    {
                        value = (byte)valueAsLong;
                        type = PrimitiveFieldTypes.ByteType;
                    }
                    else if ((valueAsLong >= short.MinValue) && (valueAsLong <= short.MaxValue))
                    {
                        value = (short)valueAsLong;
                        type = PrimitiveFieldTypes.ShortType;
                    }
                    else if ((valueAsLong >= int.MinValue) && (valueAsLong <= int.MaxValue))
                    {
                        value = (int)valueAsLong;
                        type = PrimitiveFieldTypes.IntType;
                    }
                    break;
                case FudgeTypeDictionary.DOUBLE_TYPE_ID:
                    if ((double)value == 0.0)
                    {
                        value = IndicatorType.Instance;
                        type = IndicatorFieldType.Instance;
                    }
                    break;
                case FudgeTypeDictionary.FLOAT_TYPE_ID:
                    if ((float)value == 0.0f)
                    {
                        value = IndicatorType.Instance;
                        type = IndicatorFieldType.Instance;
                    }
                    break;
                case FudgeTypeDictionary.STRING_TYPE_ID:
                    if ((string)value == "")
                    {
                        value = IndicatorType.Instance;
                        type = IndicatorFieldType.Instance;
                    }
                    break;
            }

            FudgeMsgField field = new FudgeMsgField(type, value, name, ordinal);
            fields.Add(field);
        }

        protected FudgeFieldType DetermineTypeFromValue(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Cannot determine type for null value.");
            }
            if (value is byte[])
            {
                return ByteArrayFieldType.GetBestMatch((byte[])value);
            }
            FudgeFieldType type = FudgeTypeDictionary.Instance.GetByCSharpType(value.GetType());
            if ((type == null) && (value is UnknownFudgeFieldValue))
            {
                UnknownFudgeFieldValue unknownValue = (UnknownFudgeFieldValue)value;
                type = unknownValue.Type;
            }
            return type;
        }

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
        public IList<FudgeMsgField> GetAllFields()      // TODO t0rx 2009-08-30 -- This should be IList<IFudgeField>, but we can't do this in .net 3.5, so is this OK?
        {
            return fields.AsReadOnly();
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
        public IList<IFudgeField> GetAllByOrdinal(short ordinal)
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

        public IFudgeField GetByOrdinal(short ordinal)
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

        public List<IFudgeField> GetAllByName(string name)
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

        public object GetValue(short ordinal)
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

        public object GetValue(string name, short? ordinal)
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

        public byte[] ToByteArray()
        {
            MemoryStream stream = new MemoryStream(ComputeSize(null));
            BinaryWriter bw = new BinaryWriter(stream);
            try
            {
                FudgeStreamEncoder.WriteMsg(bw, this);
                bw.Flush();
            }
            catch (IOException e)
            {
                throw new FudgeRuntimeException("Had an IOException writing to a MemoryStream.", e);        // TODO t0rx 2009-08-30 -- In Fudge-Java this is just a RuntimeException
            }
            // TODO t0rx 2009-08-30 -- Could also get an ObjectDisposedException from the BinaryWriter...

            return stream.ToArray();
        }

        public override int ComputeSize(IFudgeTaxonomy taxonomy)
        {
            int size = 0;
            // Message prefix
            size += 8;
            foreach (FudgeMsgField field in fields)
            {
                size += field.GetSize(taxonomy);
            }
            return size;
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
        public double? GetDouble(short ordinal)
        {
            return GetAsDoubleInternal(null, ordinal);
        }

        protected double? GetAsDoubleInternal(string fieldName, short? ordinal)
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
        public float? GetFloat(short ordinal)
        {
            return GetAsFloatInternal(null, ordinal);
        }

        protected float? GetAsFloatInternal(string fieldName, short? ordinal)
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
        public long? GetLong(short ordinal)
        {
            return GetAsLongInternal(null, ordinal);
        }

        protected long? GetAsLongInternal(string fieldName, short? ordinal)
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
        public int? GetInt(short ordinal)
        {
            return GetAsIntInternal(null, ordinal);
        }

        protected int? GetAsIntInternal(string fieldName, short? ordinal)
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
        public short? GetShort(short ordinal)
        {
            return GetAsShortInternal(null, ordinal);
        }

        protected short? GetAsShortInternal(string fieldName, short? ordinal)
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
        public byte? GetByte(string fieldName)
        {
            return GetAsByteInternal(fieldName, null);
        }

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>byte</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>byte</c></exception>
        public byte? GetByte(short ordinal)
        {
            return GetAsByteInternal(null, ordinal);
        }

        protected byte? GetAsByteInternal(string fieldName, short? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToByte(null);
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
        public bool? GetBoolean(short ordinal)
        {
            return GetAsBooleanInternal(null, ordinal);
        }

        protected bool? GetAsBooleanInternal(string fieldName, short? ordinal)
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
        public string GetString(short ordinal)
        {
            return GetAsStringInternal(null, ordinal);
        }

        protected string GetAsStringInternal(string fieldName, short? ordinal)
        {
            IConvertible value = GetValue(fieldName, ordinal) as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToString(null);
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

        protected object GetFirstTypedValue(short ordinal, int typeId)
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
    }
}
