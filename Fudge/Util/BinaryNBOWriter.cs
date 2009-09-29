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
    /// Like the <see cref="BinaryWriter"/>, but uses Network Byte Order for compatiblility with other languages.
    /// </summary>
    /// <remarks>Note that only the signed integer types plus float and double have been overridden.</remarks>
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
            // Big-endian
            buffer[0] = (byte)(value >> 8);
            buffer[1] = (byte)value;
            OutStream.Write(buffer, 0, 2);
        }

        public override void Write(int value)
        {
            // Big-endian
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
