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
 * -->
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fudge.Types
{
    /// <summary>
    ///  A collection of all the simple fixed-width field types that represent
    ///  primitive values.
    ///  Because these are fast-pathed inside the encoder/decoder sequence,
    ///  there's no point in breaking them out to other classes.
    /// </summary>
    public static class PrimitiveFieldTypes
    {
        /// <summary>
        /// A type definition for boolean values.
        /// </summary>
        public static readonly FudgeFieldType<bool> BooleanType = new FudgeFieldType<bool>(FudgeTypeDictionary.BOOLEAN_TYPE_ID, false, 1);

        /// <summary>
        /// A type definition for 8-bit byte values.
        /// </summary>
        public static readonly FudgeFieldType<sbyte> SByteType = new FudgeFieldType<sbyte>(FudgeTypeDictionary.SBYTE_TYPE_ID, false, 1, (sbyte i, ref FudgeFieldType t) => MinimizeIntegers(i, ref t));

        /// <summary>
        /// A type definition for signed 16-bit integers.
        /// </summary>
        public static readonly FudgeFieldType<short> ShortType = new FudgeFieldType<short>(FudgeTypeDictionary.SHORT_TYPE_ID, false, 2, (short i, ref FudgeFieldType t) => MinimizeIntegers(i, ref t));

        /// <summary>
        /// A type definition for signed 32-bit integers.
        /// </summary>
        public static readonly FudgeFieldType<int> IntType = new FudgeFieldType<int>(FudgeTypeDictionary.INT_TYPE_ID, false, 4, (int i, ref FudgeFieldType t) => MinimizeIntegers(i, ref t));

        /// <summary>
        /// A type definition for signed 64-bit integers.
        /// </summary>
        public static readonly FudgeFieldType<long> LongType = new FudgeFieldType<long>(FudgeTypeDictionary.LONG_TYPE_ID, false, 8, (long i, ref FudgeFieldType t) => MinimizeIntegers(i, ref t));

        /// <summary>
        /// A type definition for single precision (32-bit) floating point values.
        /// </summary>
        public static readonly FudgeFieldType<float> FloatType = new FudgeFieldType<float>(FudgeTypeDictionary.FLOAT_TYPE_ID, false, 4);

        /// <summary>
        /// A type definition for double precision (64-bit) floating point values.
        /// </summary>
        public static readonly FudgeFieldType<double> DoubleType = new FudgeFieldType<double>(FudgeTypeDictionary.DOUBLE_TYPE_ID, false, 8);

        #region Minimizations

        /// <summary>
        /// Delegate for reducing integers to the smallest encoding available.
        /// </summary>
        /// <param name="valueAsLong">value to reduce</param>
        /// <param name="type">original type</param>
        /// <returns>the original value, recast to a smaller type if reduction has taken place</returns>
        private static object MinimizeIntegers(long valueAsLong, ref FudgeFieldType type)
        {
            object value = valueAsLong;
            if ((valueAsLong >= sbyte.MinValue) && (valueAsLong <= sbyte.MaxValue))
            {
                value = (sbyte)valueAsLong;
                type = PrimitiveFieldTypes.SByteType;
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
            return value;
        }
        #endregion
    }
}
