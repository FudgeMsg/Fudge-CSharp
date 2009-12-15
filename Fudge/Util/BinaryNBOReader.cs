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
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Fudge.Util
{
    /// <summary>
    /// Like the <see cref="BinaryReader"/>, but uses Network Byte Order for compatiblility with other languages.
    /// </summary>
    /// <remarks>Note that only the integer types plus float and double have been overridden.</remarks>
    public class BinaryNBOReader : BinaryReader
    {
        private readonly byte[] buffer;

        /// <summary>
        /// Creates a new stream reader with the default UTF8 encoding.
        /// </summary>
        /// <param name="input">underlying input stream</param>
        public BinaryNBOReader(Stream input)
            : this(input, new UTF8Encoding())
        {
        }

        /// <summary>
        /// Creates a new stream reader with a custom encoding.
        /// </summary>
        /// <param name="input">underlying input stream</param>
        /// <param name="encoding">custom encoding</param>
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

        /// <summary>
        /// Reads a signed 16-bit integer in network byte order.
        /// </summary>
        /// <returns>value read</returns>
        public override short ReadInt16()
        {
            FillBytes(2);
            return (short)((buffer[0] << 8) | buffer[1]);
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer in network byte order.
        /// </summary>
        /// <returns>value read</returns>
        public override ushort ReadUInt16()
        {
            FillBytes(2);
            return (ushort)((buffer[0] << 8) | buffer[1]);
        }

        /// <summary>
        /// Reads a signed 32-bit integer in network byte order.
        /// </summary>
        /// <returns>value read</returns>
        public override int ReadInt32()
        {
            FillBytes(4);
            return (buffer[0] << 0x18) | (buffer[1] << 0x10) | (buffer[2] << 0x8) | buffer[3];
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer in network byte order.
        /// </summary>
        /// <returns>value read</returns>
        public override uint ReadUInt32()
        {
            FillBytes(4);
            return (uint)((buffer[0] << 0x18) | (buffer[1] << 0x10) | (buffer[2] << 0x8) | buffer[3]);
        }

        /// <summary>
        /// Reads a signed 64-bit integer in network byte order.
        /// </summary>
        /// <returns>value read</returns>
        public override long ReadInt64()
        {
            FillBytes(8);
            uint num = (uint)(((buffer[7] | (buffer[6] << 8)) | (buffer[5] << 0x10)) | (buffer[4] << 0x18));
            uint num2 = (uint)(((buffer[3] | (buffer[2] << 8)) | (buffer[1] << 0x10)) | (buffer[0] << 0x18));
            return (long)((num2 << 0x20) | num);
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer in network byte order.
        /// </summary>
        /// <returns>value read</returns>
        public override ulong ReadUInt64()
        {
            FillBytes(8);
            uint num = (uint)(((buffer[7] | (buffer[6] << 8)) | (buffer[5] << 0x10)) | (buffer[4] << 0x18));
            uint num2 = (uint)(((buffer[3] | (buffer[2] << 8)) | (buffer[1] << 0x10)) | (buffer[0] << 0x18));
            return (ulong)((num2 << 0x20) | num);
        }

        /// <summary>
        /// Reads a 32-bit (single precision) floating point value in network byte order.
        /// </summary>
        /// <returns>value read</returns>
        public override unsafe float ReadSingle()
        {
            FillBytes(4);
            uint num = (uint)((buffer[0] << 0x18) | (buffer[1] << 0x10) | (buffer[2] << 0x8) | buffer[3]);
            return *(((float*)&num));
        }

        /// <summary>
        /// Reads a 64-bit (double precision) floating point value in network byte order.
        /// </summary>
        /// <returns>value read</returns>
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
