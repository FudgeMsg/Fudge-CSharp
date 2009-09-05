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

namespace OpenGamma.Fudge
{
    /// <summary>
    /// Code relating to working with <a href="http://en.wikipedia.org/wiki/UTF-8#Modified_UTF-8">Modified UTF-8</a> data.
    /// The code here was originally in Java in <c>DataInputStream</c> and
    /// <c>DataOutputStream</c>, but it's been improved and modified
    /// to suit the use of Fudge in a superior way.
    /// </summary>
    public static class ModifiedUTF8Util
    {
        // TODO: 20090830 (t0rx): Should this actually be ModifiedUTF8Encoding and inherit from Encoding?

        public static int ModifiedUTF8Length(string str)
        {
            return ModifiedUTF8Length(str.ToCharArray(), 0, str.Length);
        }

        public static int ModifiedUTF8Length(char[] str, int index, int count)
        {
            // REVIEW wyliekir 2009-08-17 -- This was taken almost verbatim from
            // DataOutputStream.
            int utflen = 0;
            int c = 0;

            // TODO: 20090904 (t0rx): This would be more efficient if we fixed it...
            for (int i = index; i < index + count; i++)
            {
                c = str[i];
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

        public static int WriteModifiedUTF8(string str, BinaryWriter bw) //throws IOException
        {
            char[] chars = str.ToCharArray();
            int utflen = ModifiedUTF8Length(chars, 0, chars.Length);
            if (utflen > 65535)
                throw new UTFDataFormatException("Encoded string too long: " + utflen
                    + " bytes");
            byte[] bytearr = new byte[utflen];

            int count = ConvertToModifiedUTF8Bytes(chars, 0, chars.Length, bytearr, 0);
            Debug.Assert(count == utflen);
            bw.Write(bytearr);
            return count;
        }

        public static int ConvertToModifiedUTF8Bytes(char[] str, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int pos = byteIndex;
            int i = charIndex;
            char c;
            for (i = charIndex; i < charIndex + charCount; i++)
            {
                c = str[i];
                if (!((c >= 0x0001) && (c <= 0x007F)))
                    break;
                bytes[pos++] = (byte)c;
            }

            for (; i < charIndex + charCount; i++)
            {
                c = str[i];
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

        public static int GetCharCount(byte[] bytes, int byteIndex, int byteCount)
        {
            // MUST KEEP IN SYNC WITH getchars()

            int c;
            int pos = byteIndex;
            int chararr_pos = 0;

            // REVIEW kirk 2009-08-18 -- This can be optimized. We're copying the data too many
            // times. Particularly since we expect that most of the time we're reading from
            // a byte array already, duplicating it doesn't make much sense.
            while (pos < byteIndex + byteCount)
            {
                c = (int)bytes[pos] & 0xff;
                if (c > 127) break;
                pos++;
                chararr_pos++;

            }

            while (pos < byteIndex + byteCount)
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
                        if (pos > byteIndex + byteCount)
                            throw new UTFDataFormatException(
                                "malformed input: partial character at end");
                        chararr_pos++;
                        break;
                    case 14:
                        /* 1110 xxxx  10xx xxxx  10xx xxxx */
                        pos += 3;
                        if (pos > byteIndex + byteCount)
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

        public static string ReadString(BinaryReader br, int utflen) //throws IOException
        {
            byte[] bytearr = new byte[utflen];
            char[] chararr = new char[utflen];

            br.Read(bytearr, 0, utflen);

            int chararr_count = GetChars(bytearr, 0, utflen, chararr, 0);

            return new string(chararr, 0, chararr_count);
        }


        public static int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            int c, char2, char3;
            int pos = byteIndex;
            int chararr_pos = charIndex;

            // REVIEW kirk 2009-08-18 -- This can be optimized. We're copying the data too many
            // times. Particularly since we expect that most of the time we're reading from
            // a byte array already, duplicating it doesn't make much sense.
            while (pos < byteIndex + byteCount)
            {
                c = (int)bytes[pos] & 0xff;
                if (c > 127) break;
                pos++;
                chars[chararr_pos++] = (char)c;

            }

            while (pos < byteIndex + byteCount)
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
                        if (pos > byteIndex + byteCount)
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
                        if (pos > byteIndex + byteCount)
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


        // TODO: 20090830 (t0rx): Is there an existing C# exception that is more appropriate?
        public class UTFDataFormatException : Exception
        {
            public UTFDataFormatException(string message)
                : base(message)
            {
            }
        }
    }
}
