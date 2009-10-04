/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGamma.Fudge
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

        public static bool IsFixedWidth(int fieldPrefix)
        {
            return (fieldPrefix & FIELD_PREFIX_FIXED_WIDTH_MASK) != 0;
        }

        public static bool HasOrdinal(int fieldPrefix)
        {
            return (fieldPrefix & FIELD_PREFIX_ORDINAL_PROVIDED_MASK) != 0;
        }

        public static bool HasName(int fieldPrefix)
        {
            return (fieldPrefix & FIELD_PREFIX_NAME_PROVIDED_MASK) != 0;
        }

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
