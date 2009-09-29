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
using System.Diagnostics;
using OpenGamma.Fudge.Util;

namespace OpenGamma.Fudge
{
    /// <summary>
    /// Code relating to working with <a href="http://en.wikipedia.org/wiki/UTF-8#Modified_UTF-8">Modified UTF-8</a> data.
    /// </summary>
    /// <remarks>
    /// This class is kept to keep the code-base similar to Fudge-Java.  Most of the work is in <see cref="ModifiedUTF8Encoding"/>.
    /// </remarks>
    public static class ModifiedUTF8Util
    {
        public static readonly ModifiedUTF8Encoding Encoding = new ModifiedUTF8Encoding();

        public static int ModifiedUTF8Length(string str)
        {
            return Encoding.GetByteCount(str.ToCharArray(), 0, str.Length);
        }

        public static int WriteModifiedUTF8(string str, Stream output) //throws IOException
        {
            // Note that we're not prefixing with the length
            byte[] bytearr = Encoding.GetBytes(str);

            int utflen = bytearr.Length;
            if (utflen > 65535)     // Fudge has a maximum string size
                throw new ModifiedUTF8Encoding.UTFDataFormatException("Encoded string too long: " + utflen
                    + " bytes");

            output.Write(bytearr, 0, utflen);
            return utflen;
        }

        public static int GetCharCount(byte[] bytes, int byteIndex, int byteCount)
        {
            return Encoding.GetCharCount(bytes, byteIndex, byteCount);
        }

        public static string ReadString(Stream input, int utflen) //throws IOException
        {
            byte[] bytearr = new byte[utflen];

            input.Read(bytearr, 0, utflen);

            return Encoding.GetString(bytearr);
        }
    }
}
