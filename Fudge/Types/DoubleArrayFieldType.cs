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
    /// The type definition for an array of double-precision floating point numbers.
    /// </summary>
    public class DoubleArrayFieldType : FudgeArrayFieldTypeBase<double>
    {
        public static readonly DoubleArrayFieldType Instance = new DoubleArrayFieldType();

        public DoubleArrayFieldType()
            : base(FudgeTypeDictionary.DOUBLE_ARRAY_TYPE_ID, 8, (w, e) => w.Write(e), r => r.ReadDouble())
        {
        }
    }
}
