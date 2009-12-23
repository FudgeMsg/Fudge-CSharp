/* <!--
 * Copyright (C) 2009 - 2009 by OpenGamma Inc. and other contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *     
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * -->
 */
using System;
using System.IO;
using Fudge.Util;

namespace Fudge
{
    /// <summary>
    /// Code relating to working with <a href="http://en.wikipedia.org/wiki/UTF-8#Modified_UTF-8">Modified UTF-8</a> data.
    /// </summary>
    /// <remarks>
    /// This class is kept to keep the code-base similar to Fudge-Java.  Most of the work is in <see cref="ModifiedUTF8Encoding"/>.
    /// </remarks>
    public static class ModifiedUTF8Util
    {
        /// <summary>Singleton instance to avoid unnecessary construction.</summary>
        public static readonly ModifiedUTF8Encoding Encoding = new ModifiedUTF8Encoding();

        /// <summary>
        /// Returns the length, in bytes, of an encoded string.
        /// </summary>
        /// <param name="str">string to encode</param>
        /// <returns>length in bytes</returns>
        public static int ModifiedUTF8Length(string str)
        {
            int utflen = Encoding.GetByteCount(str.ToCharArray(), 0, str.Length);
            if (utflen > 65535)     // Fudge has a maximum string size
                throw new ModifiedUTF8Encoding.UTFDataFormatException("Encoded string too long: " + utflen
                    + " bytes");
            return utflen;
        }

        /// <summary>
        /// Writes a string with the modified UTF-8 encoding.
        /// </summary>
        /// <param name="str">string to write</param>
        /// <param name="output">target to write to</param>
        /// <returns>number of bytes written</returns>
        public static int WriteModifiedUTF8(string str, BinaryWriter output) //throws IOException
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

        /// <summary>
        /// Returns the number of characters encoded by a byte sequence.
        /// </summary>
        /// <param name="bytes">array of encoded data</param>
        /// <param name="byteIndex">offset into the array to start</param>
        /// <param name="byteCount">number of bytes to process</param>
        /// <returns>number of characters</returns>
        public static int GetCharCount(byte[] bytes, int byteIndex, int byteCount)
        {
            return Encoding.GetCharCount(bytes, byteIndex, byteCount);
        }

        /// <summary>
        /// Returns the number of bytes and returns the string encoded by them.
        /// </summary>
        /// <param name="input">source of data</param>
        /// <param name="utflen">number of bytes to read</param>
        /// <returns>the string</returns>
        public static string ReadString(BinaryReader input, int utflen) //throws IOException
        {
            byte[] bytearr = new byte[utflen];

            input.Read(bytearr, 0, utflen);

            return Encoding.GetString(bytearr);
        }
    }
}
