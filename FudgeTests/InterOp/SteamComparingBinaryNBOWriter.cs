using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenGamma.Fudge.Util;
using Xunit;
using Xunit.Sdk;

namespace OpenGamma.Fudge.Tests.Unit
{
    public class StreamComparingBinaryNBOWriter : BinaryNBOWriter
    {
        private readonly BinaryReader referenceReader;
        private readonly bool runToCompletion;
        private readonly StringBuilder traceBuffer = new StringBuilder();
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

                traceBuffer.AppendLine(n + ": Expected " + referenceVal.GetType().FullName + "[" + referenceVal + "] but was [" + actualVal.GetType().FullName + "[" + actualVal + "]");
                if (!runToCompletion)
                {
                    Console.Error.WriteLine(traceBuffer);
                    throw new InvalidDataException(n + ": Expected " + referenceVal.GetType().FullName + "[" + referenceVal + "] but was [" + actualVal.GetType().FullName + "[" + actualVal + "]");
                }
            }
        }

        private void Trace<T>(T actualVal, Exception e)
        {
            
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

        public override void Close()
        {
            if (traceBuffer.Length > 0)
            {
                Console.WriteLine(traceBuffer.ToString());
                throw new AssertException("Streams differed");
            }
            base.Close();
        }
    }
}