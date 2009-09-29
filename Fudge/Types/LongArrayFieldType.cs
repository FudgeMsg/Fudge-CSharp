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

namespace OpenGamma.Fudge.Types
{
    /// <summary>
    /// The type definition for an array of 64-bit integers.
    /// </summary>
    public class LongArrayFieldType : FudgeArrayFieldTypeBase<long>
    {
        public static readonly LongArrayFieldType Instance = new LongArrayFieldType();

        public LongArrayFieldType()
            : base(FudgeTypeDictionary.LONG_ARRAY_TYPE_ID, 8, (w, e) => w.Write(e), r => r.ReadInt64())
        {
        }
    }
}
