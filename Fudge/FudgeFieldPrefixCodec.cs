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

namespace Fudge
{
    /// <summary>
    /// A container for all the utilities for working with fudge field prefixes.
    /// </summary>
    public static class FudgeFieldPrefixCodec
    {
        // Yes, these are actually bytes.
        internal const int FIELD_PREFIX_FIXED_WIDTH_MASK = 0x80;
        internal const int FIELD_PREFIX_ORDINAL_PROVIDED_MASK = 0x10;
        internal const int FIELD_PREFIX_NAME_PROVIDED_MASK = 0x08;

        /// <summary>
        /// Tests if the fixed width flag is set for the field. Fixed width fields are either empty (0 bytes data),
        /// or the number of bytes defined by the field type. If the field is not a fixed width type, the encoded
        /// field will contain a width field describing its length.
        /// </summary>
        /// <param name="fieldPrefix">the field prefix byte</param>
        /// <returns>true iff the fixed width flag is set</returns>
        public static bool IsFixedWidth(int fieldPrefix)
        {
            return (fieldPrefix & FIELD_PREFIX_FIXED_WIDTH_MASK) != 0;
        }

        /// <summary>
        /// Tests if the ordinal flag is set for the field. If set, the encoded field will contain the 2 byte ordinal index of the field.
        /// </summary>
        /// <param name="fieldPrefix">the field prefix byte</param>
        /// <returns>true iff the ordinal flag is set</returns>
        public static bool HasOrdinal(int fieldPrefix)
        {
            return (fieldPrefix & FIELD_PREFIX_ORDINAL_PROVIDED_MASK) != 0;
        }

        /// <summary>
        /// Tests if the name flag is set for the field. If set, the encoded field will contain a textual description of the field.
        /// </summary>
        /// <param name="fieldPrefix">the field prefix byte</param>
        /// <returns>true iff the name flag is set</returns>
        public static bool HasName(int fieldPrefix)
        {
            return (fieldPrefix & FIELD_PREFIX_NAME_PROVIDED_MASK) != 0;
        }

        /// <summary>
        /// Returns the number of bytes used to encode the field's width if it is not a fixed width.
        /// </summary>
        /// <param name="fieldPrefix">the field prefix byte</param>
        /// <returns>the size, in bytes, of the width prefix of the fields data component</returns>
        public static int GetFieldWidthByteCount(int fieldPrefix)
        {
            fieldPrefix &= 0x60;
            int count = fieldPrefix >> 5;
            if (count == 3)
            {
                // We do this because we only have two bits to encode data in.
                // Therefore, we use binary 11 to indicate 4 bytes.
                count = 4;
            }
            return count;
        }

        /// <summary>
        /// Constructs a field prefix byte with the chosen options.
        /// </summary>
        /// <param name="fixedWidth">true iff the fixed width flag is to be set</param>
        /// <param name="varDataSize">the size, in bytes, of the variable data for non-fixed width fields</param>
        /// <param name="hasOrdinal">true iff the ordinal flag is to be set</param>
        /// <param name="hasName">true iff the name flag is to be set</param>
        /// <returns>a field prefix byte</returns>
        public static int ComposeFieldPrefix(bool fixedWidth, int varDataSize, bool hasOrdinal, bool hasName)
        {
            int varDataBits = 0;
            if (!fixedWidth)
            {
                // This is correct. This is an unsigned value for reading. See note in
                // writeFieldValue.
                if (varDataSize <= 255)
                {
                    varDataSize = 1;
                }
                else if (varDataSize <= short.MaxValue)
                {
                    varDataSize = 2;
                }
                else
                {
                    // Yes, this is right. Remember, we only have 2 bits here.
                    varDataSize = 3;
                }
                varDataBits = varDataSize << 5;
            }
            int fieldPrefix = varDataBits;
            if (fixedWidth)
            {
                fieldPrefix |= FIELD_PREFIX_FIXED_WIDTH_MASK;
            }
            if (hasOrdinal)
            {
                fieldPrefix |= FIELD_PREFIX_ORDINAL_PROVIDED_MASK;
            }
            if (hasName)
            {
                fieldPrefix |= FIELD_PREFIX_NAME_PROVIDED_MASK;
            }
            return fieldPrefix;
        }
    }
}
