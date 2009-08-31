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
    public class FudgeMsg : ISizeComputable
    {
        // TODO: 20090830 (t0rx): Finish porting FudgeMsg

        private readonly SizeCache sizeCache;
        private readonly List<FudgeMsgField> fields = new List<FudgeMsgField>();

        public FudgeMsg()
        {
            sizeCache = new SizeCache(this);
        }

        //  public FudgeMsg(FudgeMsg other) {
        //    if(other == null) {
        //      throw new NullPointerException("Cannot initialize from a null other FudgeMsg");
        //    }
        //    initializeFromByteArray(other.toByteArray());
        //  }

        //  public FudgeMsg(byte[] byteArray, FudgeTaxonomy taxonomy) {
        //    initializeFromByteArray(byteArray);
        //  }

        //  protected void initializeFromByteArray(byte[] byteArray) {
        //    ByteArrayInputStream bais = new ByteArrayInputStream(byteArray);
        //    DataInputStream is = new DataInputStream(bais);
        //    FudgeMsg other;
        //    try {
        //      other = FudgeStreamDecoder.readMsg(is);
        //    } catch (IOException e) {
        //      throw new RuntimeException("IOException thrown using ByteArrayInputStream", e);
        //    }
        //    _fields.addAll(other._fields);
        //  }

        //  public void add(FudgeField field) {
        //    if(field == null) {
        //      throw new NullPointerException("Cannot add an empty field");
        //    }
        //    _fields.add(new FudgeMsgField(field));
        //  }

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
                case FudgeTypeDictionary.SHORT_TYPE_ID:
                case FudgeTypeDictionary.INT_TYPE_ID:
                case FudgeTypeDictionary.LONG_TYPE_ID:
                    long valueAsLong = System.Convert.ToInt64(value);                  // TODO: 20090831 (t0rx): Not sure how fast this is
                    if ((valueAsLong >= byte.MinValue) && (valueAsLong <= byte.MaxValue))
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
            FudgeFieldType type = FudgeTypeDictionary.Instance.GetByCSharpType(value.GetType());
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
        public IList<FudgeMsgField> GetAllFields()      // TODO: 20090830 (t0rx): This should be IList<IFudgeField>, but we can't do this in .net 3.5, so is this OK?
        {
            return fields.AsReadOnly();
        }

        //  public FudgeField getByIndex(int index) {
        //    if(index < 0) {
        //      throw new ArrayIndexOutOfBoundsException("Cannot specify a negative index into a FudgeMsg.");
        //    }
        //    if(index >= _fields.size()) {
        //      return null;
        //    }
        //    return _fields.get(index);
        //  }

        //  // REVIEW kirk 2009-08-16 -- All of these getters are currently extremely unoptimized.
        //  // There may be an option required if we have a lot of random access to the field content
        //  // to speed things up by building an index.

        //  public List<FudgeField> getAllByOrdinal(short ordinal) {
        //    List<FudgeField> fields = new ArrayList<FudgeField>();
        //    for(FudgeMsgField field : _fields) {
        //      if((field.getOrdinal() != null) && (ordinal == field.getOrdinal())) {
        //        fields.add(field);
        //      }
        //    }
        //    return fields;
        //  }


        //  public FudgeField getByOrdinal(short ordinal) {
        //    for(FudgeMsgField field : _fields) {
        //      if((field.getOrdinal() != null) && (ordinal == field.getOrdinal())) {
        //        return field;
        //      }
        //    }
        //    return null;
        //  }

        public List<IFudgeField> GetAllByName(string name)
        {
            List<IFudgeField> results = new List<IFudgeField>();
            foreach (FudgeMsgField field in fields)
            {
                if (object.Equals(name, field.Name))        // TODO: 20090831 (t0rx): Why ObjectEquals in Fudge-Java?
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
                if (object.Equals(name, field.Name))        // TODO: 20090831 (t0rx): Why ObjectEquals in Fudge-Java?
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
                throw new FudgeRuntimeException("Had an IOException writing to a MemoryStream.", e);        // TODO: 20090830 (t0rx): In Fudge-Java this is just a RuntimeException
            }
            // TODO: 20090830 (t0rx): Could also get an ObjectDisposedException from the BinaryWriter...

            return stream.ToArray();
        }

        public int GetSize(IFudgeTaxonomy taxonomy)
        {
            return sizeCache.GetSize(taxonomy);
        }

        #region ISizeComputable Members

        public int ComputeSize(OpenGamma.Fudge.Taxon.IFudgeTaxonomy taxonomy)
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

        #endregion

        // Primitive Queries:
        public double? GetDouble(string fieldName)
        {
            return (double?)GetFirstTypedValue(fieldName, FudgeTypeDictionary.DOUBLE_TYPE_ID);
        }

        public double? GetDouble(short ordinal)
        {
            return (double?)GetFirstTypedValue(ordinal, FudgeTypeDictionary.DOUBLE_TYPE_ID);
        }

        //  public Double getAsDouble(String fieldName) {
        //    return getAsDoubleInternal(fieldName, null);
        //  }

        //  public Double getAsDouble(short ordinal) {
        //    return getAsDoubleInternal(null, ordinal);
        //  }

        //  protected Double getAsDoubleInternal(String fieldName, Short ordinal) {
        //    Object value = getValue(fieldName, ordinal);
        //    if(value instanceof Number) {
        //      Number numberValue = (Number) value;
        //      return numberValue.doubleValue();
        //    }
        //    return null;
        //  }

        // We use the name Float rather than Single to be consistent with Fudge-Java
        public float? GetFloat(string fieldName)
        {
            return (float?)GetFirstTypedValue(fieldName, FudgeTypeDictionary.FLOAT_TYPE_ID);
        }

        public float? GetFloat(short ordinal)
        {
            return (float?)GetFirstTypedValue(ordinal, FudgeTypeDictionary.FLOAT_TYPE_ID);
        }

        //  public Float getAsFloat(String fieldName) {
        //    return getAsFloatInternal(fieldName, null);
        //  }

        //  public Float getAsFloat(short ordinal) {
        //    return getAsFloatInternal(null, ordinal);
        //  }

        //  protected Float getAsFloatInternal(String fieldName, Short ordinal) {
        //    Object value = getValue(fieldName, ordinal);
        //    if(value instanceof Number) {
        //      Number numberValue = (Number) value;
        //      return numberValue.floatValue();
        //    }
        //    return null;
        //  }

        public long? GetLong(string fieldName)
        {
            return (long?)GetFirstTypedValue(fieldName, FudgeTypeDictionary.LONG_TYPE_ID);
        }

        public long? GetLong(short ordinal)
        {
            return (long?)GetFirstTypedValue(ordinal, FudgeTypeDictionary.LONG_TYPE_ID);
        }

        public long? GetAsLong(string fieldName)
        {
            return GetAsLongInternal(fieldName, null);
        }

        public long? GetAsLong(short ordinal)
        {
            return GetAsLongInternal(null, ordinal);
        }

        protected long? GetAsLongInternal(string fieldName, short? ordinal)
        {
            object value = GetValue(fieldName, ordinal);
            Number numberValue = Number.ToNumber(value);
            if (numberValue != null)
            {
                return numberValue.LongValue;
            }
            return null;
        }

        public int? GetInt(string fieldName)
        {
            return (int?)GetFirstTypedValue(fieldName, FudgeTypeDictionary.INT_TYPE_ID);
        }

        public int? GetInt(short ordinal)
        {
            return (int?)GetFirstTypedValue(ordinal, FudgeTypeDictionary.INT_TYPE_ID);
        }

        //  public Integer getAsInt(String fieldName) {
        //    return getAsIntInternal(fieldName, null);
        //  }

        //  public Integer getAsInt(short ordinal) {
        //    return getAsIntInternal(null, ordinal);
        //  }

        //  protected Integer getAsIntInternal(String fieldName, Short ordinal) {
        //    Object value = getValue(fieldName, ordinal);
        //    if(value instanceof Number) {
        //      Number numberValue = (Number) value;
        //      return numberValue.intValue();
        //    }
        //    return null;
        //  }

        public short? GetShort(string fieldName)
        {
            return (short?)GetFirstTypedValue(fieldName, FudgeTypeDictionary.SHORT_TYPE_ID);
        }

        public short? GetShort(short ordinal)
        {
            return (short?)GetFirstTypedValue(ordinal, FudgeTypeDictionary.SHORT_TYPE_ID);
        }

        //  public Short getAsShort(String fieldName) {
        //    return getAsShortInternal(fieldName, null);
        //  }

        //  public Short getAsShort(short ordinal) {
        //    return getAsShortInternal(null, ordinal);
        //  }

        //  protected Short getAsShortInternal(String fieldName, Short ordinal) {
        //    Object value = getValue(fieldName, ordinal);
        //    if(value instanceof Number) {
        //      Number numberValue = (Number) value;
        //      return numberValue.shortValue();
        //    }
        //    return null;
        //  }

        public byte? GetByte(string fieldName)
        {
            return (byte?)GetFirstTypedValue(fieldName, FudgeTypeDictionary.BYTE_TYPE_ID);
        }

        public byte? GetByte(short ordinal)
        {
            return (byte?)GetFirstTypedValue(ordinal, FudgeTypeDictionary.BYTE_TYPE_ID);
        }

        //  public Byte getAsByte(String fieldName) {
        //    return getAsByteInternal(fieldName, null);
        //  }

        //  public Byte getAsByte(short ordinal) {
        //    return getAsByteInternal(null, ordinal);
        //  }

        //  protected Byte getAsByteInternal(String fieldName, Short ordinal) {
        //    Object value = getValue(fieldName, ordinal);
        //    if(value instanceof Number) {
        //      Number numberValue = (Number) value;
        //      return numberValue.byteValue();
        //    }
        //    return null;
        //  }

        public string GetString(string fieldName)
        {
            return (string)GetFirstTypedValue(fieldName, FudgeTypeDictionary.STRING_TYPE_ID);
        }

        public string GetString(short ordinal)
        {
            return (string)GetFirstTypedValue(ordinal, FudgeTypeDictionary.STRING_TYPE_ID);
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
                if ((ordinal == field.Ordinal)
                  && (field.Type.TypeId == typeId))
                {
                    return field.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Helps us simulate Number in Java
        /// </summary>
        private class Number
        {
            private readonly long longVal;
            private readonly double doubleVal;
            private readonly bool isLong;

            public Number(object o)
            {
            }

            private Number(long longVal, double doubleVal, bool isLong)
            {
                this.longVal = longVal;
                this.doubleVal = doubleVal;
                this.isLong = isLong;
            }

            public static bool IsNumber(object o)
            {
                long? longVal;
                double? doubleVal;
                return TryCreate(o, out longVal, out doubleVal);
            }

            /// <summary>
            /// Creates a <see cref="Number"/> instance if the given object is numerical, or returns <c>null</c> if not.
            /// </summary>
            /// <remarks>Using this method is more efficient than calling <see cref="IsNumber"/> and then constructing if valid.</remarks>
            /// <param name="o">Object to manipulate</param>
            /// <returns>A <see cref="Number"/> instance, or <c>Null</c> if the object is not numerical</returns>
            public static Number ToNumber(object o)
            {
                long? longVal;
                double? doubleVal;
                if (!TryCreate(o, out longVal, out doubleVal))
                {
                    return null;
                }
                if (longVal.HasValue)
                    return new Number(longVal.Value, 0.0, true);
                else
                    return new Number(0, doubleVal.Value, false);
            }

            public long LongValue
            {
                get { return isLong ? longVal : (long)doubleVal; }
            }

            private static bool TryCreate(object o, out long? longVal, out double? doubleVal)
            {
                longVal = null;
                doubleVal = null;
                if (o == null)
                {
                    return false;
                }
                TypeCode typeCode = Convert.GetTypeCode(o);
                switch (typeCode)
                {
                    case TypeCode.Byte:
                        longVal = (byte)o;
                        return true;
                    case TypeCode.Int16:
                        longVal = (short)o;
                        return true;
                    case TypeCode.Int32:
                        longVal = (long)(int)o;
                        return true;
                    case TypeCode.Int64:
                        longVal = (long)o;
                        return true;
                    case TypeCode.SByte:
                        longVal = (byte)o;
                        return true;
                    case TypeCode.UInt16:
                        longVal = (long)(ushort)o;
                        return true;
                    case TypeCode.UInt32:
                        longVal = (long)(uint)o;
                        return true;
                    case TypeCode.UInt64:
                        longVal = (long)(ulong)o;
                        return true;
                    case TypeCode.Double:
                        doubleVal = (double)o;
                        return true;
                    case TypeCode.Single:
                        doubleVal = (float)o;
                        return true;
                    default:
                        doubleVal = null;
                        return false;
                }
            }
        }
    }
}
