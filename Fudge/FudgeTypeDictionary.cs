﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge.Types;

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
        private readonly Dictionary<Type, FudgeFieldType> typesByCSharpType = new Dictionary<Type, FudgeFieldType>();       // Also used as synchronisation lock

        public void AddType(FudgeFieldType type, params Type[] alternativeTypes)
        {
            if (type == null)
            {
                throw new ArgumentNullException("Must not provide a null FudgeFieldType to add.");
            }
            lock (typesByCSharpType)
            {
                int newLength = Math.Max(type.TypeId + 1, typesById.Length);
                var newArray = new FudgeFieldType[newLength];
                typesById.CopyTo(newArray, 0);
                newArray[type.TypeId] = type;
                typesById = newArray;

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

        public FudgeFieldType GetByTypeId(int typeId)
        {
            if (typeId >= typesById.Length)
            {
                return null;
            }
            return typesById[typeId];
        }

        // --------------------------
        // STANDARD FUDGE FIELD TYPES
        // --------------------------
        public const byte BOOLEAN_TYPE_ID = 0;
        public const byte BYTE_TYPE_ID = 1;
        public const byte SHORT_TYPE_ID = 2;
        public const byte INT_TYPE_ID = 3;
        public const byte LONG_TYPE_ID = 4;
        public const byte SHORT_ARRAY_TYPE_ID = 5;
        public const byte INT_ARRAY_TYPE_ID = 6;
        public const byte LONG_ARRAY_TYPE_ID = 7;
        public const byte INDICATOR_TYPE_ID = 8;
        public const byte FLOAT_TYPE_ID = 17;           // We use the name Float rather than Single to be consistent with Fudge-Java
        public const byte FLOAT_ARRAY_TYPE_ID = 18;
        public const byte DOUBLE_TYPE_ID = 19;
        public const byte DOUBLE_ARRAY_TYPE_ID = 20;
        public const byte BYTE_ARRAY_TYPE_ID = 21;
        public const byte STRING_TYPE_ID = 22;
        public const byte FUDGE_MSG_TYPE_ID = 23;

        static FudgeTypeDictionary()
        {
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
            Instance.AddType(ByteArrayFieldType.Instance);
            Instance.AddType(StringFieldType.Instance);
            Instance.AddType(FudgeMsgFieldType.Instance);
        }
    }
}
