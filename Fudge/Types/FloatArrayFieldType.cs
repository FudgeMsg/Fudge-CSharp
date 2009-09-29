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
    /// The type definition for an array of single-precision floating point numbers.
    /// </summary>
    public class FloatArrayFieldType : FudgeArrayFieldTypeBase<float>
    {
        public static readonly FloatArrayFieldType Instance = new FloatArrayFieldType();

        public FloatArrayFieldType()
            : base(FudgeTypeDictionary.FLOAT_ARRAY_TYPE_ID, 4, (w, e) => w.Write(e), r => r.ReadSingle())
        {
        }
    }
}
