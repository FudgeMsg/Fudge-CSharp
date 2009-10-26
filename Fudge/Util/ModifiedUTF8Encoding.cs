/**
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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fudge.Util
{
    /// <summary>
    /// Encoding to support working with <a href="http://en.wikipedia.org/wiki/UTF-8#Modified_UTF-8">Modified UTF-8</a> data.
    /// </summary>
    public class ModifiedUTF8Encoding : Encoding
    {
        // See the .net implementation of UTF8Encoding for what needs doing.

        public override int GetByteCount(char[] chars, int index, int count)
        {
            // REVIEW wyliekir 2009-08-17 -- This was taken almost verbatim from
            // DataOutputStream.
            int utflen = 0;
            int c = 0;

            for (int i = index; i < index + count; i++)
            {
                c = chars[i];
                if ((c >= 0x0001) && (c <= 0x007F))
                {
                    utflen++;
                }
                else if (c > 0x07FF)
                {
                    utflen += 3;
                }
                else
                {
                    utflen += 2;
                }
            }
            return utflen;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int pos = byteIndex;
            int i = charIndex;
            char c;
            for (i = charIndex; i < charIndex + charCount; i++)
            {
                c = chars[i];
                if (!((c >= 0x0001) && (c <= 0x007F)))
                    break;
                bytes[pos++] = (byte)c;
            }

            for (; i < charIndex + charCount; i++)
            {
                c = chars[i];
                if ((c >= 0x0001) && (c <= 0x007F))
                {
                    bytes[pos++] = (byte)c;

                }
                else if (c > 0x07FF)
                {
                    bytes[pos++] = (byte)(0xE0 | ((c >> 12) & 0x0F));
                    bytes[pos++] = (byte)(0x80 | ((c >> 6) & 0x3F));
                    bytes[pos++] = (byte)(0x80 | ((c >> 0) & 0x3F));
                }
                else
                {
                    bytes[pos++] = (byte)(0xC0 | ((c >> 6) & 0x1F));
                    bytes[pos++] = (byte)(0x80 | ((c >> 0) & 0x3F));
                }
            }
            return pos - byteIndex;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            // MUST KEEP IN SYNC WITH GetChars()

            int c;
            int pos = index;
            int chararr_pos = 0;
            int end = index + count;

            // REVIEW kirk 2009-08-18 -- This can be optimized. We're copying the data too many
            // times. Particularly since we expect that most of the time we're reading from
            // a byte array already, duplicating it doesn't make much sense.
            while (pos < end)
            {
                c = (int)bytes[pos] & 0xff;
                if (c > 127) break;
                pos++;
                chararr_pos++;

            }

            while (pos < end)
            {
                c = (int)bytes[pos] & 0xff;
                switch (c >> 4)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        /* 0xxxxxxx*/
                        pos++;
                        chararr_pos++;
                        break;
                    case 12:
                    case 13:
                        /* 110x xxxx   10xx xxxx*/
                        pos += 2;
                        if (pos > end)
                            throw new UTFDataFormatException(
                                "malformed input: partial character at end");
                        chararr_pos++;
                        break;
                    case 14:
                        /* 1110 xxxx  10xx xxxx  10xx xxxx */
                        pos += 3;
                        if (pos > end)
                            throw new UTFDataFormatException(
                                "malformed input: partial character at end");
                        chararr_pos++;
                        break;
                    default:
                        /* 10xx xxxx,  1111 xxxx */
                        throw new UTFDataFormatException(
                            "malformed input around byte " + pos);
                }
            }
            return chararr_pos;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            // MUST BE KEPT IN SYNC WITH GetCharCount()
            int c, char2, char3;
            int pos = byteIndex;
            int chararr_pos = charIndex;
            int end = byteIndex + byteCount;

            // REVIEW kirk 2009-08-18 -- This can be optimized. We're copying the data too many
            // times. Particularly since we expect that most of the time we're reading from
            // a byte array already, duplicating it doesn't make much sense.
            while (pos < end)
            {
                c = (int)bytes[pos] & 0xff;
                if (c > 127) break;
                pos++;
                chars[chararr_pos++] = (char)c;

            }

            while (pos < end)
            {
                c = (int)bytes[pos] & 0xff;
                switch (c >> 4)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        /* 0xxxxxxx*/
                        pos++;
                        chars[chararr_pos++] = (char)c;
                        break;
                    case 12:
                    case 13:
                        /* 110x xxxx   10xx xxxx*/
                        pos += 2;
                        if (pos > end)
                            throw new UTFDataFormatException(
                                "malformed input: partial character at end");
                        char2 = (int)bytes[pos - 1];
                        if ((char2 & 0xC0) != 0x80)
                            throw new UTFDataFormatException(
                                "malformed input around byte " + pos);
                        chars[chararr_pos++] = (char)(((c & 0x1F) << 6) |
                                                        (char2 & 0x3F));
                        break;
                    case 14:
                        /* 1110 xxxx  10xx xxxx  10xx xxxx */
                        pos += 3;
                        if (pos > end)
                            throw new UTFDataFormatException(
                                "malformed input: partial character at end");
                        char2 = (int)bytes[pos - 2];
                        char3 = (int)bytes[pos - 1];
                        if (((char2 & 0xC0) != 0x80) || ((char3 & 0xC0) != 0x80))
                            throw new UTFDataFormatException(
                                "malformed input around byte " + (pos - 1));
                        chars[chararr_pos++] = (char)(((c & 0x0F) << 12) |
                                                        ((char2 & 0x3F) << 6) |
                                                        ((char3 & 0x3F) << 0));
                        break;
                    default:
                        /* 10xx xxxx,  1111 xxxx */
                        throw new UTFDataFormatException(
                            "malformed input around byte " + pos);
                }
            }
            // The number of chars produced may be less than utflen
            return chararr_pos - charIndex;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount * 3;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }

        // TODO t0rx 2009-08-30 -- Is there an existing C# exception that is more appropriate?
        public class UTFDataFormatException : Exception
        {
            public UTFDataFormatException(string message)
                : base(message)
            {
            }
        }
    }
}
