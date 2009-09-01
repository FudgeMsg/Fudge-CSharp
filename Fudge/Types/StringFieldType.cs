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
    /// The type definition for a Modified UTF-8 encoded string.
    /// </summary>
    public class StringFieldType : FudgeFieldType<string>
    {
        public static readonly StringFieldType Instance = new StringFieldType();

        public StringFieldType()
            : base(FudgeTypeDictionary.STRING_TYPE_ID, true, 0)
        {
        }

        public override int GetVariableSize(string value, IFudgeTaxonomy taxonomy)
        {
            return ModifiedUTF8Util.ModifiedUTF8Length(value);
        }

        public override string ReadTypedValue(BinaryReader input, int dataSize)
        {
            return ModifiedUTF8Util.ReadString(input, dataSize);
        }

        public override void WriteValue(BinaryWriter output, string value, IFudgeTaxonomy taxonomy, short taxonomyId)
        {
            ModifiedUTF8Util.WriteModifiedUTF8(value, output);
        }
    }
}
