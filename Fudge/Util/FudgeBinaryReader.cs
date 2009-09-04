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

namespace OpenGamma.Fudge.Util
{
    /// <summary>
    /// FudgeBinaryReader provides the wire encoding for primitive types.
    /// </summary>
    /// <remarks>
    /// The default <see cref="BinaryReader"/> uses little-endian integer encoding, and UTF8, whereas Fudge always
    /// uses Network Byte Order (i.e. big-endian) and modified UTF-8
    /// </remarks>
    public class FudgeBinaryReader : BinaryNBOReader
    {
        public FudgeBinaryReader(Stream input)
            : base(input, new ModifiedUTF8Encoding())
        {
        }
    }
}
