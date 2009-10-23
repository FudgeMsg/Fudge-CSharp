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
using System.IO;
using System.Text;
using Fudge.Util;
using Xunit;
using Xunit.Sdk;

namespace Fudge.Tests.Unit
{
    public class StreamComparingBinaryNBOWriter : BinaryNBOWriter
    {
        private readonly BinaryReader referenceReader;
        private readonly bool runToCompletion;
        private readonly StringBuilder traceBuffer = new StringBuilder();
        private bool errored = false;
        private int n = 0;
       
        public StreamComparingBinaryNBOWriter(BinaryReader referenceReader, Stream input, bool runToCompletion)
            : this(referenceReader, input, new UTF8Encoding(), runToCompletion)
        {
        }

        public StreamComparingBinaryNBOWriter(BinaryReader referenceReader, Stream input, Encoding encoding, bool runToCompletion)
            : base(input, encoding)
        {
            this.referenceReader = referenceReader;
            this.runToCompletion = runToCompletion;
        }


        private void Trace<T>(T referenceVal, T actualVal)
        {
            if (referenceVal.Equals(actualVal))
            {
                traceBuffer.AppendLine(n+": "+actualVal.GetType().FullName +  "["+actualVal+"]");
            } 
            else
            {
                errored = true;
                traceBuffer.AppendLine(n + ": Expected " + referenceVal.GetType().FullName + "[" + referenceVal + "] but was [" + actualVal.GetType().FullName + "[" + actualVal + "]");
                if (!runToCompletion)
                {
                    Console.Error.WriteLine(traceBuffer);
                    throw new InvalidDataException(n + ": Expected " + referenceVal.GetType().FullName + "[" + referenceVal + "] but was [" + actualVal.GetType().FullName + "[" + actualVal + "]");
                }
            }
        }

        private void Trace<T>(T[] referenceArray, T[] actualArray)
        {
            if (referenceArray.Length != actualArray.Length)
            {
                errored = true;
                traceBuffer.AppendLine(n + ": Expected " + referenceArray.GetType().FullName + "[length " + referenceArray.Length + "] but was [" + actualArray.GetType().FullName + "[length " + actualArray.Length + "]");
                if (!runToCompletion)
                {
                    Console.Error.WriteLine(traceBuffer);
                    throw new InvalidDataException(n + ": Expected " + referenceArray.GetType().FullName + "[length " + referenceArray.Length + "] but was [" + actualArray.GetType().FullName + "[length " + actualArray.Length + "]");
                }
            }
            traceBuffer.Append(n + ": " + actualArray.GetType().FullName + ": ");
            for (int i = 0; i < referenceArray.Length; i++)
            {
                if (referenceArray[i].Equals(actualArray[i]))
                {
                    traceBuffer.Append("[" + actualArray[i] + "]");
                }
                else
                {
                    errored = true;
                    traceBuffer.Append("Expected [" + referenceArray[i] + "] but was [" + actualArray[i] + "]");
                    if (!runToCompletion)
                    {
                        Console.Error.WriteLine(traceBuffer);
                        throw new InvalidDataException(n + ": " + referenceArray.GetType().FullName + "Element " + i + ": Expected [" + referenceArray[i] + "] but was [" + actualArray[i] + "]");
                    }
                }
            }
            traceBuffer.AppendLine();
        }

        private void Trace<T>(T actualVal, Exception e)
        {

            errored = true;
            traceBuffer.AppendLine(n + ": Expected End of Stream but was [" + actualVal.GetType().FullName + "[" +
                                       actualVal + "]");
            if (!runToCompletion)
            {
                Console.Error.WriteLine(traceBuffer);
                throw e;
            }
        }

        public override void Write(bool value)
        {
            try
            {
                Trace(referenceReader.ReadBoolean(), value);
            } 
            catch (EndOfStreamException e)
            {
                Trace(value, e);
            }
            base.Write(value);
            n++;
        }

        public override void Write(sbyte value)
        {
            try {
                Trace(referenceReader.ReadSByte(), value);
            } 
            catch (EndOfStreamException e)
            {
                Trace(value, e);
            }
            base.Write(value);
            n++;
        }

        public override void Write(byte value)
        {
            try
            {
                Trace(referenceReader.ReadByte(), value);
            }
            catch (EndOfStreamException e)
            {
                Trace(value, e);
            }
            base.Write(value);
            n++;
        }

       public override void Write(short value)
       {
           try
           {
               Trace(referenceReader.ReadInt16(), value);
           }
           catch (EndOfStreamException e)
           {
               Trace(value, e);
           }
           base.Write(value);
           n += 2;
       }

        public override void Write(int value)
        {
            try
            {
                Trace(referenceReader.ReadInt32(), value);
            }
            catch (EndOfStreamException e)
            {
                Trace(value, e);
            }
            base.Write(value);
            n += 4;
        }

        public override void Write(long value)
        {
            try
            {
                Trace(referenceReader.ReadInt64(), value);
            }
            catch (EndOfStreamException e)
            {
                Trace(value, e);
            }
            base.Write(value);
            n += 8;
        }

        public override void Write(float value)
        {
            try
            {
                Trace(referenceReader.ReadSingle(), value);
            }
            catch (EndOfStreamException e)
            {
                Trace(value, e);
            }
            
            base.Write(value);
            n += 4;
        }

        public override void Write(double value)
        {
            try
            {
                Trace(referenceReader.ReadDouble(), value);
            }
            catch (EndOfStreamException e)
            {
                Trace(value, e);
            }
            base.Write(value);
            n += 8;
        }

        public override void Write(byte[] value)
        {
            try
            {
                Trace(referenceReader.ReadBytes(value.Length), value);
            }
            catch (EndOfStreamException e)
            {
                Trace(value, e);
            }
            base.Write(value);
            n += value.Length;
        }

        public override void Write(byte[] buffer, int index, int count)
        {
            byte[] copy = new byte[count];
            Array.Copy(buffer, index, copy, 0, count);
            Write(copy);
        }

        public override void Close()
        {
            if (errored)
            {
                Console.WriteLine(traceBuffer.ToString());
                throw new AssertException("Streams differed");
            }
            base.Close();
        }
    }
}