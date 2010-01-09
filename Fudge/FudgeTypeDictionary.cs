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
        private volatile FudgeFieldType[] typesById = new FudgeFieldType[0];
        private volatile UnknownFudgeFieldType[] unknownTypesById = new UnknownFudgeFieldType[0];
        private readonly Dictionary<Type, FudgeFieldType> typesByCSharpType = new Dictionary<Type, FudgeFieldType>();
        private readonly ReaderWriterLock rwLock = new ReaderWriterLock();      // Synchronisation lock around typesByCSharpType

        /// <summary>
        /// Creates a new dictionary with the default Fudge types. After construction custom types can be registered using <c>AddType</c>.
        /// </summary>
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
            AddType(DateFieldType.Instance);
        }

        /// <summary>
        /// Registers a type definition with this dictionary. Additional .NET types can be passed that will be translated to the
        /// type referenced in the type definition. If a type with same numeric type identifier is already in the dictionary it
        /// will be replaced.
        /// </summary>
        /// <param name="type">type definition</param>
        /// <param name="alternativeTypes">alternative .NET types that map to this type</param>
        public void AddType(FudgeFieldType type, params Type[] alternativeTypes)
        {
            if (type == null)
            {
                throw new ArgumentNullException("Must not provide a null FudgeFieldType to add.");
            }

            rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {                
                if (!(type is ISecondaryFieldType))       // TODO 2009-09-12 t0rx -- Don't like this as a way of testing
                {
                    int newLength = Math.Max(type.TypeId + 1, typesById.Length);
                    var newArray = new FudgeFieldType[newLength];
                    typesById.CopyTo(newArray, 0);
                    newArray[type.TypeId] = type;
                    typesById = newArray;
                }

                // TODO 2009-12-14 Andrew -- the secondary type mechanism needs review; not sure how best to do it in Java and I would like to keep the APIs similar in spirit

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

        /// <summary>
        /// Returns the type definition most appropiate for a value type.
        /// </summary>
        /// <param name="csharpType">type of a value</param>
        /// <returns>type definition</returns>
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

        // TODO 2009-12-14 Andrew -- should the name above refer to a .NET type, or should we have wrappers for the other languages?

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

        /// <summary>
        /// Returns a type definition for a type ID not defined within this dictionary. This can be used
        /// to allow the message to be partially processed, preserving the unknown aspects of it.
        /// </summary>
        /// <param name="typeId">type ID</param>
        /// <returns>type definition</returns>
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

        /// <summary>Predefined constant for IndicatorType - refer to the Fudge encoding specification.</summary>
        public const byte INDICATOR_TYPE_ID = 0;
        /// <summary>Predefined constant for PrimitiveFieldTypes.BooleanType - refer to the Fudge encoding specification.</summary>
        public const byte BOOLEAN_TYPE_ID = 1;
        /// <summary>Predefined constant for PrimitiveFieldTypes.SByteType - refer to the Fudge encoding specification.</summary>
        public const byte SBYTE_TYPE_ID = 2;
        /// <summary>Predefined constant for PrimitiveFieldTypes.ShortType - refer to the Fudge encoding specification.</summary>
        public const byte SHORT_TYPE_ID = 3;
        /// <summary>Predefined constant for PrimitiveFieldTypes.IntType - refer to the Fudge encoding specification.</summary>
        public const byte INT_TYPE_ID = 4;
        /// <summary>Predefined constant for PrimitiveFieldTypes.LongType - refer to the Fudge encoding specification.</summary>
        public const byte LONG_TYPE_ID = 5;
        /// <summary>Predefined constant for ByteArrayFieldType - refer to the Fudge encoding specification.</summary>
        public const byte BYTE_ARRAY_TYPE_ID = 6;
        /// <summary>Predefined constant for ShortArrayFieldType - refer to the Fudge encoding specification.</summary>
        public const byte SHORT_ARRAY_TYPE_ID = 7;
        /// <summary>Predefined constant for IntArrayFieldType - refer to the Fudge encoding specification.</summary>
        public const byte INT_ARRAY_TYPE_ID = 8;
        /// <summary>Predefined constant for LongArrayFieldType - refer to the Fudge encoding specification.</summary>
        public const byte LONG_ARRAY_TYPE_ID = 9;
        /// <summary>Predefined constant for PrimitiveFieldTypes.FloatType - refer to the Fudge encoding specification.</summary>
        public const byte FLOAT_TYPE_ID = 10;
        /// <summary>Predefined constant for PrimitiveFieldTypes.DoubleType - refer to the Fudge encoding specification.</summary>
        public const byte DOUBLE_TYPE_ID = 11;
        /// <summary>Predefined constant for FloatArrayFieldType - refer to the Fudge encoding specification.</summary>
        public const byte FLOAT_ARRAY_TYPE_ID = 12;
        /// <summary>Predefined constant for DoubleArrayFieldType - refer to the Fudge encoding specification.</summary>
        public const byte DOUBLE_ARRAY_TYPE_ID = 13;
        /// <summary>Predefined constant for StringFieldType - refer to the Fudge encoding specification.</summary>
        public const byte STRING_TYPE_ID = 14;
        // Indicators for controlling stack-based sub-message expressions:
        /// <summary>Predefined constant for FudgeMsgFieldType - refer to the Fudge encoding specification.</summary>
        public const byte FUDGE_MSG_TYPE_ID = 15;
        // End message indicator type removed as unnecessary, so no 16.
        // The fixed-width byte arrays:
        /// <summary>Predefined constant for a 4-byte array - refer to the Fudge encoding specification.</summary>
        public const byte BYTE_ARR_4_TYPE_ID = 17;
        /// <summary>Predefined constant for a 8-byte array - refer to the Fudge encoding specification.</summary>
        public const byte BYTE_ARR_8_TYPE_ID = 18;
        /// <summary>Predefined constant for a 16-byte array - refer to the Fudge encoding specification.</summary>
        public const byte BYTE_ARR_16_TYPE_ID = 19;
        /// <summary>Predefined constant for a 20-byte array - refer to the Fudge encoding specification.</summary>
        public const byte BYTE_ARR_20_TYPE_ID = 20;
        /// <summary>Predefined constant for a 32-byte array - refer to the Fudge encoding specification.</summary>
        public const byte BYTE_ARR_32_TYPE_ID = 21;
        /// <summary>Predefined constant for a 64-byte array - refer to the Fudge encoding specification.</summary>
        public const byte BYTE_ARR_64_TYPE_ID = 22;
        /// <summary>Predefined constant for a 128-byte array - refer to the Fudge encoding specification.</summary>
        public const byte BYTE_ARR_128_TYPE_ID = 23;
        /// <summary>Predefined constant for a 256-byte array - refer to the Fudge encoding specification.</summary>
        public const byte BYTE_ARR_256_TYPE_ID = 24;
        /// <summary>Predefined constant for a 512-byte array - refer to the Fudge encoding specification.</summary>
        public const byte BYTE_ARR_512_TYPE_ID = 25;
        /// <summary>Predefined constant for a pure date - refer to the Fudge encoding specification.</summary>
        public const byte DATE_TYPE_ID = 26;
        /// <summary>Predefined constant for a pure time - refer to the Fudge encoding specification.</summary>
        public const byte TIME_TYPE_ID = 27;
        /// <summary>Predefined constant for date and time- refer to the Fudge encoding specification.</summary>
        public const byte DATETIME_TYPE_ID = 28;
    }
}
