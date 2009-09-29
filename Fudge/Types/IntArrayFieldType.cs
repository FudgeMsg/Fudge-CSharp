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
    /// The type definition for an array of 32-bit integers.
    /// </summary>
    public class IntArrayFieldType : FudgeArrayFieldTypeBase<int>
    {
        public static readonly IntArrayFieldType Instance = new IntArrayFieldType();

        public IntArrayFieldType()
            : base(FudgeTypeDictionary.INT_ARRAY_TYPE_ID, 4, (w, e) => w.Write(e), r => r.ReadInt32())
        {
        }
    }
}
