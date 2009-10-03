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
        public static FudgeMsg ReadMsg(BinaryReader br) //throws IOException
        {
            return ReadMsg(br, (TaxonomyResolver)null);
        }

        public static FudgeMsg ReadMsg(BinaryReader br, ITaxonomyResolver taxonomyResolver) //throws IOException
        {
            if (taxonomyResolver == null)
                return ReadMsg(br, (TaxonomyResolver)null);
            else
                return ReadMsg(br, id => taxonomyResolver.ResolveTaxonomy(id));
        }

        public static FudgeMsg ReadMsg(BinaryReader br, TaxonomyResolver taxonomyResolver) //throws IOException
        {
            CheckInputStream(br);
            int nRead = 0;
            short taxonomyId = br.ReadInt16();
            nRead += 2;
            short nFields = br.ReadInt16();
            nRead += 2;
            int size = br.ReadInt32();
            nRead += 4;

            FudgeMsg msg = new FudgeMsg();
            for (int i = 0; i < nFields; i++)
            {
                nRead += ReadField(br, msg);
            }

            if ((size > 0) && (nRead != size))
            {
                throw new FudgeRuntimeException("Expected to read " + size + " but only had " + nRead + " in message.");      // TODO t0rx 2009-08-31 -- This is just RuntimeException in Fudge-Java
            }

            if (taxonomyResolver != null)
            {
                IFudgeTaxonomy taxonomy = taxonomyResolver(taxonomyId);
                if (taxonomy != null)
                {
                    msg.SetNamesFromTaxonomy(taxonomy);
                }
            }

            return msg;
        }

        /// <summary>
        /// Reads data about a field, and adds it to the message as a new field.
        /// </summary>
        /// <param name="?"></param>
        /// <returns>The number of bytes read.</returns>
        public static int ReadField(BinaryReader br, FudgeMsg msg) //throws IOException
        {
            CheckInputStream(br);
            int nRead = 0;

            byte fieldPrefix = br.ReadByte();
            nRead++;
            bool fixedWidth = (fieldPrefix & FudgeStreamEncoder.FIELD_PREFIX_FIXED_WIDTH_MASK) != 0;
            bool hasOrdinal = (fieldPrefix & FudgeStreamEncoder.FIELD_PREFIX_ORDINAL_PROVIDED_MASK) != 0;
            bool hasName = (fieldPrefix & FudgeStreamEncoder.FIELD_PREFIX_NAME_PROVIDED_MASK) != 0;
            int varSizeBytes = 0;
            if (!fixedWidth)
            {
                varSizeBytes = (fieldPrefix << 1) >> 6;
            }

            int typeId = br.ReadByte();
            nRead++;

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
                name = ModifiedUTF8Util.ReadString(br.BaseStream, nameSize);
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

            msg.Add(type, fieldValue, name, ordinal);

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
