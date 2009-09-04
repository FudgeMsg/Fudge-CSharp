/**
* Copyright (C) 2009 - 2009 by OpenGamma Inc.
*
* Please see distribution for license.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenGamma.Fudge.Taxon;
using System.Diagnostics;

namespace OpenGamma.Fudge
{
    public class FudgeStreamEncoder
    {
        // Yes, this is a byte.
        internal const int FIELD_PREFIX_FIXED_WIDTH_MASK = 0x80;
        internal const int FIELD_PREFIX_ORDINAL_PROVIDED_MASK = 0x10;
        internal const int FIELD_PREFIX_NAME_PROVIDED_MASK = 0x08;

        public static void WriteMsg(BinaryWriter bw, FudgeMsg msg) //throws IOException
        {
            WriteMsg(bw, msg, null, (short)0);
        }

        public static void WriteMsg(BinaryWriter bw, FudgeMsg msg, IFudgeTaxonomy taxonomy, short taxonomyId)// throws IOException
        {
            CheckOutputStream(bw);
            if (msg == null)
            {
                throw new ArgumentNullException("Must provide a message to output.");
            }
            int nWritten = 0;
            int msgSize = msg.GetSize(taxonomy);
            nWritten += WriteMsgHeader(bw, taxonomyId, msg.GetNumFields(), msgSize);
            foreach (IFudgeField field in msg.GetAllFields())
            {
                nWritten += WriteField(bw, field.Type, field.Value, field.Ordinal, field.Name, taxonomy, taxonomyId);
            }
            Debug.Assert(nWritten == msgSize, "Expected to write " + msgSize + " but actually wrote " + nWritten);
        }

        public static int WriteMsgHeader(BinaryWriter bw, int taxonomy, short nFields, int messageSize)// throws IOException
        {
            CheckOutputStream(bw);
            int nWritten = 0;
            bw.Write((short)taxonomy);
            nWritten += 2;
            bw.Write((short)nFields);
            nWritten += 2;
            bw.Write((int)messageSize);
            nWritten += 4;
            return nWritten;
        }

        public static int WriteField(BinaryWriter bw, FudgeFieldType type, object value, short? ordinal, string name)// throws IOException
        {
            return WriteField(bw, type, value, ordinal, name, null, (short)0);
        }

        public static int WriteField(BinaryWriter bw, FudgeFieldType type,
              object value, short? ordinal, string name,
              IFudgeTaxonomy taxonomy, short taxonomyId)// throws IOException
        {
            CheckOutputStream(bw);
            if (type == null)
            {
                throw new ArgumentNullException("Must provide the type of data encoded.");
            }
            if (value == null)
            {
                throw new ArgumentNullException("Must provide the value to encode.");
            }

            // First, normalize the name/ordinal bit
            if ((taxonomy != null) && (name != null))
            {
                short? ordinalFromTaxonomy = taxonomy.GetFieldOrdinal(name);
                if (ordinalFromTaxonomy != null)
                {
                    if ((ordinal != null) && !object.Equals(ordinalFromTaxonomy, ordinal))
                    {
                        // In this case, we've been provided an ordinal, but it doesn't match the
                        // one from the taxonomy. We have to assume the user meant what they were doing,
                        // and not do anything.
                    }
                    else
                    {
                        ordinal = ordinalFromTaxonomy;
                        name = null;
                    }
                }
            }

            int nWritten = 0;
            int valueSize = type.IsVariableSize ? type.GetVariableSize(value, taxonomy) : type.FixedSize;
            int fieldPrefix = ComposeFieldPrefix(!type.IsVariableSize, valueSize, (ordinal != null), (name != null));
            bw.Write((byte)fieldPrefix);
            nWritten++;
            bw.Write((byte)type.TypeId);
            nWritten++;
            if (ordinal != null)
            {
                bw.Write(ordinal.Value);
                nWritten += 2;
            }
            if (name != null)
            {
                int utf8size = ModifiedUTF8Util.ModifiedUTF8Length(name);
                if (utf8size > 0xFF)
                {
                    throw new ArgumentOutOfRangeException("UTF-8 encoded field name cannot exceed 255 characters. Name \"" + name + "\" is " + utf8size + " bytes encoded.");
                }
                bw.Write((byte)utf8size);
                nWritten++;
                nWritten += ModifiedUTF8Util.WriteModifiedUTF8(name, bw);
            }
            nWritten += WriteFieldValue(bw, type, value, valueSize, taxonomy, taxonomyId);
            return nWritten;
        }

        protected static int WriteFieldValue(BinaryWriter bw, FudgeFieldType type, object value, int valueSize, IFudgeTaxonomy taxonomy, short taxonomyId) //throws IOException
        {
            // Note that we fast-path types for which at compile time we know how to handle
            // in an optimized way. This is because this particular method is known to
            // be a massive hot-spot for performance.
            int nWritten = 0;
            switch (type.TypeId)
            {
                case FudgeTypeDictionary.BOOLEAN_TYPE_ID:
                    bw.Write((bool)value);
                    nWritten = 1;
                    break;
                case FudgeTypeDictionary.BYTE_TYPE_ID:
                    bw.Write((byte)value);
                    nWritten = 1;
                    break;
                case FudgeTypeDictionary.SHORT_TYPE_ID:
                    bw.Write((short)value);
                    nWritten = 2;
                    break;
                case FudgeTypeDictionary.INT_TYPE_ID:
                    bw.Write((int)value);
                    nWritten = 4;
                    break;
                case FudgeTypeDictionary.LONG_TYPE_ID:
                    bw.Write((long)value);
                    nWritten = 8;
                    break;
                case FudgeTypeDictionary.FLOAT_TYPE_ID:
                    bw.Write((float)value);
                    nWritten = 4;
                    break;
                case FudgeTypeDictionary.DOUBLE_TYPE_ID:
                    bw.Write((double)value);
                    nWritten = 8;
                    break;
            }
            if (nWritten == 0)
            {
                if (type.IsVariableSize)
                {
                    // This is correct. We read this using a .readUnsignedByte(), so we can go to
                    // 255 here.
                    if (valueSize <= 255)
                    {
                        bw.Write((byte)valueSize);
                        nWritten = valueSize + 1;
                    }
                    else if (valueSize <= short.MaxValue)
                    {
                        bw.Write((short)valueSize);
                        nWritten = valueSize + 2;
                    }
                    else
                    {
                        bw.Write((int)valueSize);
                        nWritten = valueSize + 4;
                    }
                    type.WriteValue(bw, value, taxonomy, taxonomyId);
                }
            }
            return nWritten;
        }

        protected static int ComposeFieldPrefix(bool fixedWidth, int varDataSize, bool hasOrdinal, bool hasName)
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

        protected static void CheckOutputStream(BinaryWriter bw)
        {
            if (bw == null)
            {
                throw new ArgumentNullException("Must specify a BinaryWriter for processing.");
            }
        }
    }
}
