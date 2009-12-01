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
using Fudge.Types;
using System.Diagnostics;
using System.Threading;

namespace Fudge
{
    /// <summary>
    /// Contains all the <see cref="FudgeFieldType"/> definitions for a particular
    /// Fudge installation.
    /// You control it through your <see cref="FudgeContext"/>.
    /// </summary>
    public sealed class FudgeTypeDictionary
    {
        internal static readonly FudgeTypeDictionary Instance = new FudgeTypeDictionary();

        private volatile FudgeFieldType[] typesById = new FudgeFieldType[0];
        private volatile UnknownFudgeFieldType[] unknownTypesById = new UnknownFudgeFieldType[0];
        private readonly Dictionary<Type, FudgeFieldType> typesByCSharpType = new Dictionary<Type, FudgeFieldType>();
        private readonly ReaderWriterLock rwLock = new ReaderWriterLock();      // Synchronisation lock around typesByCSharpType

        public FudgeTypeDictionary()
        {
            AddType(ByteArrayFieldType.Length4Instance);
            AddType(ByteArrayFieldType.Length8Instance);
            AddType(ByteArrayFieldType.Length16Instance);
            AddType(ByteArrayFieldType.Length20Instance);
            AddType(ByteArrayFieldType.Length32Instance);
            AddType(ByteArrayFieldType.Length64Instance);
            AddType(ByteArrayFieldType.Length128Instance);
            AddType(ByteArrayFieldType.Length256Instance);
            AddType(ByteArrayFieldType.Length512Instance);

            AddType(PrimitiveFieldTypes.BooleanType);
            AddType(PrimitiveFieldTypes.SByteType);
            AddType(PrimitiveFieldTypes.ShortType);
            AddType(PrimitiveFieldTypes.IntType);
            AddType(PrimitiveFieldTypes.LongType);
            AddType(PrimitiveFieldTypes.FloatType);
            AddType(ShortArrayFieldType.Instance);
            AddType(IntArrayFieldType.Instance);
            AddType(LongArrayFieldType.Instance);
            AddType(IndicatorFieldType.Instance);
            AddType(FloatArrayFieldType.Instance);
            AddType(PrimitiveFieldTypes.DoubleType);
            AddType(DoubleArrayFieldType.Instance);
            AddType(ByteArrayFieldType.VariableSizedInstance);
            AddType(StringFieldType.Instance);
            AddType(FudgeMsgFieldType.Instance);
            AddType(StringArrayFieldType.Instance);
        }

        public void AddType(FudgeFieldType type, params Type[] alternativeTypes)
        {
            if (type == null)
            {
                throw new ArgumentNullException("Must not provide a null FudgeFieldType to add.");
            }

            rwLock.AcquireWriterLock(Timeout.Infinite);
            try
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
            finally
            {
                rwLock.ReleaseWriterLock();
            }
        }

        public FudgeFieldType GetByCSharpType(Type csharpType)
        {
            if (csharpType == null)
            {
                return null;
            }
            
            rwLock.AcquireReaderLock(Timeout.Infinite);

            FudgeFieldType result = null;
            typesByCSharpType.TryGetValue(csharpType, out result);
            
            rwLock.ReleaseReaderLock();
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
            if ((unknownTypesById.Length <= typeId) || (unknownTypesById[typeId] == null))
            {
                int newLength = Math.Max(typeId + 1, unknownTypesById.Length); 
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
        public const byte SBYTE_TYPE_ID = 2;
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
        // End message indicator type removed as unnecessary, so no 16.
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
        public const byte STRING_ARRAY_TYPE_ID = 26;
    }
}
