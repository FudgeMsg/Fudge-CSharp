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
            // REVIEW wyliekir 2009-08-17 -- This was taken almost verbatim from
            // DataOutputStream.
            int strlen = str.Length;
            int utflen = 0;
            int c = 0;

            /* use charAt instead of copying String to char array */
            for (int i = 0; i < strlen; i++)
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
            // REVIEW wyliekir 2009-08-17 -- This was taken almost verbatim from
            // DataOutputStream.
            int strlen = str.Length;
            int utflen = 0;
            int c, count = 0;

            /* use charAt instead of copying String to char array */
            int i;
            for (i = 0; i < strlen; i++)
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
            if (utflen > 65535)
                throw new UTFDataFormatException("Encoded string too long: " + utflen
                    + " bytes");

            byte[] bytearr = new byte[utflen];

            i = 0;
            for (i = 0; i < strlen; i++)
            {
                c = str[i];
                if (!((c >= 0x0001) && (c <= 0x007F)))
                    break;
                bytearr[count++] = (byte)c;
            }

            for (; i < strlen; i++)
            {
                c = str[i];
                if ((c >= 0x0001) && (c <= 0x007F))
                {
                    bytearr[count++] = (byte)c;

                }
                else if (c > 0x07FF)
                {
                    bytearr[count++] = (byte)(0xE0 | ((c >> 12) & 0x0F));
                    bytearr[count++] = (byte)(0x80 | ((c >> 6) & 0x3F));
                    bytearr[count++] = (byte)(0x80 | ((c >> 0) & 0x3F));
                }
                else
                {
                    bytearr[count++] = (byte)(0xC0 | ((c >> 6) & 0x1F));
                    bytearr[count++] = (byte)(0x80 | ((c >> 0) & 0x3F));
                }
            }
            Debug.Assert(count == utflen);
            bw.Write(bytearr);
            return utflen;
        }

        public static string ReadString(BinaryReader br, int utflen) //throws IOException
        {
            // REVIEW kirk 2009-08-18 -- This can be optimized. We're copying the data too many
            // times. Particularly since we expect that most of the time we're reading from
            // a byte array already, duplicating it doesn't make much sense.
            byte[] bytearr = new byte[utflen];
            char[] chararr = new char[utflen];

            int c, char2, char3;
            int count = 0;
            int chararr_count = 0;

            br.Read(bytearr, 0, utflen);

            while (count < utflen)
            {
                c = (int)bytearr[count] & 0xff;
                if (c > 127) break;
                count++;
                chararr[chararr_count++] = (char)c;
            }

            while (count < utflen)
            {
                c = (int)bytearr[count] & 0xff;
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
                        count++;
                        chararr[chararr_count++] = (char)c;
                        break;
                    case 12:
                    case 13:
                        /* 110x xxxx   10xx xxxx*/
                        count += 2;
                        if (count > utflen)
                            throw new UTFDataFormatException(
                                "malformed input: partial character at end");
                        char2 = (int)bytearr[count - 1];
                        if ((char2 & 0xC0) != 0x80)
                            throw new UTFDataFormatException(
                                "malformed input around byte " + count);
                        chararr[chararr_count++] = (char)(((c & 0x1F) << 6) |
                                                        (char2 & 0x3F));
                        break;
                    case 14:
                        /* 1110 xxxx  10xx xxxx  10xx xxxx */
                        count += 3;
                        if (count > utflen)
                            throw new UTFDataFormatException(
                                "malformed input: partial character at end");
                        char2 = (int)bytearr[count - 2];
                        char3 = (int)bytearr[count - 1];
                        if (((char2 & 0xC0) != 0x80) || ((char3 & 0xC0) != 0x80))
                            throw new UTFDataFormatException(
                                "malformed input around byte " + (count - 1));
                        chararr[chararr_count++] = (char)(((c & 0x0F) << 12) |
                                                        ((char2 & 0x3F) << 6) |
                                                        ((char3 & 0x3F) << 0));
                        break;
                    default:
                        /* 10xx xxxx,  1111 xxxx */
                        throw new UTFDataFormatException(
                            "malformed input around byte " + count);
                }
            }
            // The number of chars produced may be less than utflen
            return new string(chararr, 0, chararr_count);
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
