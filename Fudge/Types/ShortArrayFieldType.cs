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
    /// The type definition for an array of 16-bit integers.
    /// </summary>
    public class ShortArrayFieldType : FudgeArrayFieldTypeBase<short>
    {
        public static readonly ShortArrayFieldType Instance = new ShortArrayFieldType();

        public ShortArrayFieldType()
            : base(FudgeTypeDictionary.SHORT_ARRAY_TYPE_ID, 2, (w, e) => w.Write(e), r => r.ReadInt16())
        {
        }
    }
}
