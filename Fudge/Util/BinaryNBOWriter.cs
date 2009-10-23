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
using System.IO;

namespace Fudge.Util
{
    /// <summary>
    /// Like the <see cref="BinaryWriter"/>, but uses Network Byte Order for compatiblility with other languages.
    /// </summary>
    /// <remarks>Note that only the integer types plus float and double have been overridden.</remarks>
    public class BinaryNBOWriter : BinaryWriter
    {
        private readonly byte[] buffer = new byte[10];

        public BinaryNBOWriter(Stream output)
            : base(output)
        {
        }

        public BinaryNBOWriter(Stream output, Encoding encoding)
            : base(output, encoding)
        {
        }

        public override void Write(short value)
        {
            buffer[0] = (byte)(value >> 8);
            buffer[1] = (byte)value;
            OutStream.Write(buffer, 0, 2);
        }

        public override void Write(ushort value)
        {
            buffer[0] = (byte)(value >> 8);
            buffer[1] = (byte)value;
            OutStream.Write(buffer, 0, 2);
        }

        public override void Write(int value)
        {
            buffer[0] = (byte)(value >> 0x18);
            buffer[1] = (byte)(value >> 0x10);
            buffer[2] = (byte)(value >> 0x08);
            buffer[3] = (byte)value;
            OutStream.Write(buffer, 0, 4);
        }

        public override void Write(uint value)
        {
            buffer[0] = (byte)(value >> 0x18);
            buffer[1] = (byte)(value >> 0x10);
            buffer[2] = (byte)(value >> 0x08);
            buffer[3] = (byte)value;
            OutStream.Write(buffer, 0, 4);
        }

        public override void Write(long value)
        {
            buffer[0] = (byte)(value >> 0x38);
            buffer[1] = (byte)(value >> 0x30);
            buffer[2] = (byte)(value >> 0x28);
            buffer[3] = (byte)(value >> 0x20);
            buffer[4] = (byte)(value >> 0x18);
            buffer[5] = (byte)(value >> 0x10);
            buffer[6] = (byte)(value >> 0x08);
            buffer[7] = (byte)(value >> 0x00);
            OutStream.Write(buffer, 0, 8);
        }

        public override void Write(ulong value)
        {
            buffer[0] = (byte)(value >> 0x38);
            buffer[1] = (byte)(value >> 0x30);
            buffer[2] = (byte)(value >> 0x28);
            buffer[3] = (byte)(value >> 0x20);
            buffer[4] = (byte)(value >> 0x18);
            buffer[5] = (byte)(value >> 0x10);
            buffer[6] = (byte)(value >> 0x08);
            buffer[7] = (byte)(value >> 0x00);
            OutStream.Write(buffer, 0, 8);
        }

        public override unsafe void Write(float value)
        {
            uint num = *((uint*)&value);
            buffer[0] = (byte)(num >> 0x18);
            buffer[1] = (byte)(num >> 0x10);
            buffer[2] = (byte)(num >> 0x08);
            buffer[3] = (byte)(num >> 0x00);
            OutStream.Write(buffer, 0, 4);
        }

        public override unsafe void Write(double value)
        {
            ulong num = *((ulong*)&value);
            buffer[0] = (byte)(num >> 0x38);
            buffer[1] = (byte)(num >> 0x30);
            buffer[2] = (byte)(num >> 0x28);
            buffer[3] = (byte)(num >> 0x20);
            buffer[4] = (byte)(num >> 0x18);
            buffer[5] = (byte)(num >> 0x10);
            buffer[6] = (byte)(num >> 0x08);
            buffer[7] = (byte)(num >> 0x00);
            OutStream.Write(buffer, 0, 8);
        }
    }
}
