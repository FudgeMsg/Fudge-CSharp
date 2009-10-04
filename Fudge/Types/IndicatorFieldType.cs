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
    public class IndicatorFieldType : FudgeFieldType<IndicatorType>
    {
        public static readonly IndicatorFieldType Instance = new IndicatorFieldType();

        public IndicatorFieldType()
            : base(FudgeTypeDictionary.INDICATOR_TYPE_ID, false, 0)
        {
        }

        public override IndicatorType ReadTypedValue(BinaryReader input, int dataSize) //throws IOException
        {
            return IndicatorType.Instance;
        }

        public override void WriteValue(BinaryWriter output, IndicatorType value, IFudgeTaxonomy taxonomy) //throws IOException
        {
            // Intentional no-op.
        }
    }
}
