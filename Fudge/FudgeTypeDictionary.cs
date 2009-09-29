using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge.Types;
using System.Diagnostics;

namespace OpenGamma.Fudge
{
    /// <summary>
    /// Contains all the <see cref="FudgeFieldType"/> definitions for a particular
    /// Fudge installation.
    /// This class will usually be used as a classic Singleton, although the constructor
    /// is public so that it can be used in a Dependency Injection framework. 
    /// </summary>
    public sealed class FudgeTypeDictionary
    {
        public static readonly FudgeTypeDictionary Instance = new FudgeTypeDictionary();

        private volatile FudgeFieldType[] typesById = new FudgeFieldType[0];
        private volatile UnknownFudgeFieldType[] unknownTypesById = new UnknownFudgeFieldType[0];
        private readonly Dictionary<Type, FudgeFieldType> typesByCSharpType = new Dictionary<Type, FudgeFieldType>();       // Also used as synchronisation lock

        public void AddType(FudgeFieldType type, params Type[] alternativeTypes)
        {
            if (type == null)
            {
                throw new ArgumentNullException("Must not provide a null FudgeFieldType to add.");
            }
            lock (typesByCSharpType)
            {
                if (!(type is ISecondaryFieldType))       // TODO t0rx 2009-09-12 -- Don't like this as a way of testing
                {
                    int newLength = Math.Max(type.TypeId + 1, typesById.Length);
                    var newArray = new FudgeFieldType[newLength];
                    typesById.CopyTo(newArray, 0);
                    newArray[type.TypeId] = type;
                    typesById = newArray;
                }

                typesByCSharpType[type.CSharpType] = type;
                foreach (Type alternativeType in alternativeTypes)
                {
                    typesByCSharpType[alternativeType] = type;
                }
            }
        }

        public FudgeFieldType GetByCSharpType(Type csharpType)
        {
            if (csharpType == null)
            {
                return null;
            }
            FudgeFieldType result;
            lock (typesByCSharpType)
            {
                if (!typesByCSharpType.TryGetValue(csharpType, out result))
                    return null;
            }
            return result;
        }

        /// <summary>
        /// Obtain a <em>known</em> type by the type ID specified.
        /// For processing unhandled variable-width field types, this method will return
        /// <c>null</c>, and <see cref="GetUnknownType(int)"/> should be used if unhandled-type
        /// processing is desired.        /// </summary>
        public FudgeFieldType GetByTypeId(int typeId)
        {
            if (typeId >= typesById.Length)
            {
                return null;
            }
            return typesById[typeId];
        }

        public UnknownFudgeFieldType GetUnknownType(int typeId)
        {
            int newLength = Math.Max(typeId + 1, unknownTypesById.Length);
            if ((unknownTypesById.Length < newLength) || (unknownTypesById[typeId] == null))
            {
                lock (unknownTypesById)
                {
                    if ((unknownTypesById.Length < newLength) || (unknownTypesById[typeId] == null))
                    {
                        UnknownFudgeFieldType[] newArray = new UnknownFudgeFieldType[newLength];
                        unknownTypesById.CopyTo(newArray, 0);
                        newArray[typeId] = new UnknownFudgeFieldType(typeId);
                        unknownTypesById = newArray;
                    }
                }
            }
            Debug.Assert(unknownTypesById[typeId] != null);
            return unknownTypesById[typeId];
        }

        // --------------------------
        // STANDARD FUDGE FIELD TYPES
        // --------------------------
        public const byte INDICATOR_TYPE_ID = 0;
        public const byte BOOLEAN_TYPE_ID = 1;
        public const byte BYTE_TYPE_ID = 2;
        public const byte SHORT_TYPE_ID = 3;
        public const byte INT_TYPE_ID = 4;
        public const byte LONG_TYPE_ID = 5;
        public const byte BYTE_ARRAY_TYPE_ID = 6;
        public const byte SHORT_ARRAY_TYPE_ID = 7;
        public const byte INT_ARRAY_TYPE_ID = 8;
        public const byte LONG_ARRAY_TYPE_ID = 9;
        public const byte FLOAT_TYPE_ID = 10;
        public const byte DOUBLE_TYPE_ID = 11;
        public const byte FLOAT_ARRAY_TYPE_ID = 12;
        public const byte DOUBLE_ARRAY_TYPE_ID = 13;
        public const byte STRING_TYPE_ID = 14;
        // Indicators for controlling stack-based sub-message expressions:
        public const byte FUDGE_MSG_TYPE_ID = 15;
        public const byte END_FUDGE_MSG_TYPE_ID = 16;
        // The fixed-width byte arrays:
        public const byte BYTE_ARR_4_TYPE_ID = 17;
        public const byte BYTE_ARR_8_TYPE_ID = 18;
        public const byte BYTE_ARR_16_TYPE_ID = 19;
        public const byte BYTE_ARR_20_TYPE_ID = 20;
        public const byte BYTE_ARR_32_TYPE_ID = 21;
        public const byte BYTE_ARR_64_TYPE_ID = 22;
        public const byte BYTE_ARR_128_TYPE_ID = 23;
        public const byte BYTE_ARR_256_TYPE_ID = 24;
        public const byte BYTE_ARR_512_TYPE_ID = 25;

        static FudgeTypeDictionary()
        {
            // We have to add the fixed width byte array types first, so that the last
            // one can override.
            Instance.AddType(ByteArrayFieldType.Length4Instance);
            Instance.AddType(ByteArrayFieldType.Length8Instance);
            Instance.AddType(ByteArrayFieldType.Length16Instance);
            Instance.AddType(ByteArrayFieldType.Length20Instance);
            Instance.AddType(ByteArrayFieldType.Length32Instance);
            Instance.AddType(ByteArrayFieldType.Length64Instance);
            Instance.AddType(ByteArrayFieldType.Length128Instance);
            Instance.AddType(ByteArrayFieldType.Length256Instance);
            Instance.AddType(ByteArrayFieldType.Length512Instance);    

            Instance.AddType(PrimitiveFieldTypes.BooleanType);
            Instance.AddType(PrimitiveFieldTypes.ByteType);
            Instance.AddType(PrimitiveFieldTypes.ShortType);
            Instance.AddType(PrimitiveFieldTypes.IntType);
            Instance.AddType(PrimitiveFieldTypes.LongType);
            Instance.AddType(PrimitiveFieldTypes.FloatType);
            Instance.AddType(ShortArrayFieldType.Instance);
            Instance.AddType(IntArrayFieldType.Instance);
            Instance.AddType(LongArrayFieldType.Instance);
            Instance.AddType(IndicatorFieldType.Instance);
            Instance.AddType(FloatArrayFieldType.Instance);
            Instance.AddType(PrimitiveFieldTypes.DoubleType);
            Instance.AddType(DoubleArrayFieldType.Instance);
            Instance.AddType(ByteArrayFieldType.VariableSizedInstance);
            Instance.AddType(StringFieldType.Instance);
            Instance.AddType(FudgeMsgFieldType.Instance);
        }
    }
}
