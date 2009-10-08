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
    public class FudgeStreamDecoder
    {
        public static FudgeMsgEnvelope ReadMsg(BinaryReader br) //throws IOException
        {
            return ReadMsg(br, (TaxonomyResolver)null);
        }

        public static FudgeMsgEnvelope ReadMsg(BinaryReader br, ITaxonomyResolver taxonomyResolver) //throws IOException
        {
            if (taxonomyResolver == null)
                return ReadMsg(br, (TaxonomyResolver)null);
            else
                return ReadMsg(br, id => taxonomyResolver.ResolveTaxonomy(id));
        }

        public static FudgeMsgEnvelope ReadMsg(BinaryReader br, TaxonomyResolver taxonomyResolver) //throws IOException
        {
            CheckInputStream(br);
            int nRead = 0;
            /*int processingDirectives = */
            br.ReadByte();
            nRead += 1;
            int version = br.ReadByte();
            nRead += 1;
            short taxonomyId = br.ReadInt16();
            nRead += 2;
            int size = br.ReadInt32();
            nRead += 4;

            IFudgeTaxonomy taxonomy = null;
            if (taxonomyResolver != null)
            {
                taxonomy = taxonomyResolver(taxonomyId);
            }

            FudgeMsg msg = new FudgeMsg();
            // note that this is size-nRead because the size is for the whole envelope, including the header which we've already read in.
            nRead += ReadMsgFields(br, size - nRead, taxonomy, msg);

            if ((size > 0) && (nRead != size))
            {
                throw new FudgeRuntimeException("Expected to read " + size + " but only had " + nRead + " in message.");      // TODO t0rx 2009-08-31 -- This is just RuntimeException in Fudge-Java
            }

            FudgeMsgEnvelope envelope = new FudgeMsgEnvelope(msg, version);
            return envelope;
        }

        public static int ReadMsgFields(BinaryReader br, int size, IFudgeTaxonomy taxonomy, FudgeMsg msg)   // throws IOException
        {
            if (msg == null)
            {
                throw new ArgumentNullException("msg", "Must specify a message to populate with fields.");
            }
            int nRead = 0;
            while (nRead < size)
            {
                byte fieldPrefix = br.ReadByte();
                nRead++;
                int typeId = br.ReadByte();
                nRead++;
                nRead += ReadField(br, msg, fieldPrefix, typeId);
            }
            if (taxonomy != null)
            {
                msg.SetNamesFromTaxonomy(taxonomy);
            }
            return nRead;
        }


        /// <summary>
        /// Reads data about a field, and adds it to the message as a new field.
        /// </summary>
        /// <param name="?"></param>
        /// <returns>The number of bytes read.</returns>
        public static int ReadField(BinaryReader br, FudgeMsg msg, byte fieldPrefix, int typeId) //throws IOException
        {
            CheckInputStream(br);
            int nRead = 0;

            bool fixedWidth = FudgeFieldPrefixCodec.IsFixedWidth(fieldPrefix);
            bool hasOrdinal = FudgeFieldPrefixCodec.HasOrdinal(fieldPrefix);
            bool hasName = FudgeFieldPrefixCodec.HasName(fieldPrefix);
            int varSizeBytes = 0;
            if (!fixedWidth)
            {
                varSizeBytes = (fieldPrefix << 1) >> 6;
            }

            short? ordinal = null;
            if (hasOrdinal)
            {
                ordinal = br.ReadInt16();
                nRead += 2;
            }

            String name = null;
            if (hasName)
            {
                int nameSize = br.ReadByte();
                nRead++;
                name = ModifiedUTF8Util.ReadString(br, nameSize);
                nRead += nameSize;
            }

            FudgeFieldType type = FudgeTypeDictionary.Instance.GetByTypeId(typeId);
            if (type == null)
            {
                if (fixedWidth)
                {
                    throw new FudgeRuntimeException("Unknown fixed width type " + typeId + " for field " + ordinal + ":" + name + " cannot be handled.");       // TODO t0rx 2009-09-09 -- In Fudge-Java this is just RuntimeException
                }
                type = FudgeTypeDictionary.Instance.GetUnknownType(typeId);
            }
            int varSize = 0;
            if (!fixedWidth)
            {
                switch (varSizeBytes)
                {
                    case 0: varSize = 0; break;
                    case 1: varSize = br.ReadByte(); nRead += 1; break;
                    case 2: varSize = br.ReadInt16(); nRead += 2; break;      // TODO t0rx 2009-08-31 -- Review whether this should be signed or not
                    // Yes, this is right. We only have 2 bits here.
                    case 3: varSize = br.ReadInt32(); nRead += 4; break;
                    default:
                        throw new FudgeRuntimeException("Illegal number of bytes indicated for variable width encoding: " + varSizeBytes);        // TODO t0rx 2009-08-31 -- In Fudge-Java this is just a RuntimeException
                }

            }
            object fieldValue = ReadFieldValue(br, type, varSize);
            if (fixedWidth)
            {
                nRead += type.FixedSize;
            }
            else
            {
                nRead += varSize;
            }

            msg.Add(name, ordinal, type, fieldValue);

            return nRead;
        }

        public static object ReadFieldValue(BinaryReader br, FudgeFieldType type, int varSize) //throws IOException
        {
            Debug.Assert(type != null);
            Debug.Assert(br != null);

            // Special fast-pass for known field types
            switch (type.TypeId)
            {
                case FudgeTypeDictionary.BOOLEAN_TYPE_ID:
                    return br.ReadBoolean();
                case FudgeTypeDictionary.SBYTE_TYPE_ID:
                    return br.ReadSByte();
                case FudgeTypeDictionary.SHORT_TYPE_ID:
                    return br.ReadInt16();
                case FudgeTypeDictionary.INT_TYPE_ID:
                    return br.ReadInt32();
                case FudgeTypeDictionary.LONG_TYPE_ID:
                    return br.ReadInt64();
                case FudgeTypeDictionary.FLOAT_TYPE_ID:
                    return br.ReadSingle();
                case FudgeTypeDictionary.DOUBLE_TYPE_ID:
                    return br.ReadDouble();
            }

            return type.ReadValue(br, varSize);
        }

        protected static void CheckInputStream(BinaryReader br)
        {
            if (br == null)
            {
                throw new ArgumentNullException("Must specify a BinaryReader for processing.");
            }
        }
    }
}
