/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge.Taxon;
using System.IO;

namespace OpenGamma.Fudge.Types
{
    /// <summary>
    /// A type class for handling variable sized field values where the type
    /// isn't available in the current <see cref="FudgeTypeDictionary"/>.
    /// </summary>
    public class UnknownFudgeFieldType : FudgeFieldType<UnknownFudgeFieldValue>
    {

        public UnknownFudgeFieldType(int typeId)
            : base(typeId, true, 0)
        {
        }

        public override int GetVariableSize(UnknownFudgeFieldValue value, IFudgeTaxonomy taxonomy)
        {
            return value.Contents.Length;
        }

        public override UnknownFudgeFieldValue ReadTypedValue(BinaryReader input, int dataSize)
        {
            byte[] contents = new byte[dataSize];
            input.Read(contents, 0, dataSize);
            return new UnknownFudgeFieldValue(contents, this);
        }

        public override void WriteValue(BinaryWriter output, UnknownFudgeFieldValue value, IFudgeTaxonomy taxonomy)
        {
            output.Write(value.Contents);
        }
    }
}
