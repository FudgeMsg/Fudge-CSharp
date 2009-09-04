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
    /// Like the <see cref="BinaryReader"/>, but uses Network Byte Order for compatiblility with other languages.
    /// </summary>
    /// <remarks>Note that only the signed integer types plus float and double have been overridden.</remarks>
    public class BinaryNBOReader : BinaryReader
    {
        private readonly byte[] buffer;

        public BinaryNBOReader(Stream input)
            : this(input, new UTF8Encoding())
        {
        }

        public BinaryNBOReader(Stream input, Encoding encoding)
            : base(input, encoding)
        {
            int maxByteCount = encoding.GetMaxByteCount(1);
            if (maxByteCount < 0x10)
            {
                maxByteCount = 0x10;
            }
            buffer = new byte[maxByteCount];
        }

        public override short ReadInt16()
        {
            FillBytes(2);
            return (short)((buffer[0] << 8) | buffer[1]);
        }

        public override int ReadInt32()
        {
            FillBytes(4);
            return (buffer[0] << 0x18) | (buffer[1] << 0x10) | (buffer[2] << 0x8) | buffer[3];
        }

        public override long ReadInt64()
        {
            FillBytes(8);
            uint num = (uint)(((buffer[7] | (buffer[6] << 8)) | (buffer[5] << 0x10)) | (buffer[4] << 0x18));
            uint num2 = (uint)(((buffer[3] | (buffer[2] << 8)) | (buffer[1] << 0x10)) | (buffer[0] << 0x18));
            return (long)((num2 << 0x20) | num);
        }

        public override unsafe float ReadSingle()
        {
            FillBytes(4);
            uint num = (uint)((buffer[0] << 0x18) | (buffer[1] << 0x10) | (buffer[2] << 0x8) | buffer[3]);
            return *(((float*)&num));
        }

        public override unsafe double ReadDouble()
        {
            FillBytes(8);
            uint num = (uint)(buffer[7] | (buffer[6] << 8) | (buffer[5] << 0x10) | (buffer[4] << 0x18));
            uint num2 = (uint)(buffer[3] | (buffer[2] << 8) | (buffer[1] << 0x10) | (buffer[0] << 0x18));
            ulong num3 = ((ulong)num2 << 0x20) | (ulong)num;
            return *(((double*)&num3));
        }


        private void FillBytes(int numBytes)
        {
            // BinaryReader.FillBuffer is accessible but the buffer isn't, so we have to do it ourselves
            int offset = 0;
            int num2 = 0;
            if (BaseStream == null)
            {
                throw new ObjectDisposedException("File closed");
            }
            if (numBytes == 1)
            {
                num2 = BaseStream.ReadByte();
                if (num2 == -1)
                {
                    throw new EndOfStreamException("Read beyond end of stream");
                }
                buffer[0] = (byte)num2;
            }
            else
            {
                do
                {
                    num2 = BaseStream.Read(buffer, offset, numBytes - offset);
                    if (num2 == 0)
                    {
                        throw new EndOfStreamException("Read beyond end of stream");
                    }
                    offset += num2;
                }
                while (offset < numBytes);
            }
        }
    }
}
