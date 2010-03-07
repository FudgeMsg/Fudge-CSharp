/* <!--
 * Copyright (C) 2009 - 2010 by OpenGamma Inc. and other contributors.
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
using Fudge.Taxon;
using Fudge.Util;
using System.Diagnostics;
using Fudge.Types;

namespace Fudge.Encodings
{
    /// <summary>
    /// <c>FudgeEncodedStreamWriter</c> writes Fudge messages using the Fudge Encoding Specification.
    /// </summary>
    /// <remarks>
    /// The full specification can be found at http://wiki.fudgemsg.org/display/FDG/Encoding+Specification
    /// </remarks>
    public class FudgeEncodedStreamWriter : IFudgeStreamWriter
    {
        // Implementation notes
        // ====================
        // An incoming top-most message is written to a temporary MemoryStream until it is complete and then
        // written out to the real output stream.  The reason for this is that the field header for a message
        // includes its size, but it is more efficient to work this out by writing it than to recurse down
        // the message asking it.  See EndSubMessage().
        // To slightly complicate things, the number of butes that writing the size takes depends on the size
        // (e.g. < 255 takes just one byte), so we have to also account for this as we copy from the MemoryStream
        // to the output stream.  See WriteMemStreamToOutput().
        private readonly FudgeContext context;
        private BinaryWriter outputWriter;
        private MemoryStream memStream;
        private FudgeBinaryWriter memWriter;
        private readonly Stack<State> stack = new Stack<State>();       // Maintains state for stack of sub-messages we're in
        private readonly List<int> sizePositions = new List<int>();     // List of all the places in memStream where we have a size that we'll need to shrink
        private IFudgeTaxonomy taxonomy = null;
        private const int CopyBufferSize = 200;     // Based on some empirical testing
        private byte[] copyBuffer;
        private const int EnvelopeVersion = 0;

        /// <summary>
        /// Constructs a new <see cref="FudgeEncodedStreamWriter"/> using a given <see cref="FudgeContext"/>.
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> to use to write messages.</param>
        public FudgeEncodedStreamWriter(FudgeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.context = context;
            this.copyBuffer = new byte[CopyBufferSize];
        }

        /// <summary>
        /// Constructs a new <see cref="FudgeEncodedStreamWriter"/> using a given <see cref="FudgeContext"/>.
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> to use to write messages.</param>
        /// <param name="stream">Output stream to write to.</param>
        public FudgeEncodedStreamWriter(FudgeContext context, Stream stream)
            : this(context)
        {
            Reset(stream);
        }

        /// <summary>
        /// Gets and sets the taxonomy ID used for messages.
        /// </summary>
        public short? TaxonomyId
        {
            get;
            set;
        }

        /// <summary>
        /// Resets the <see cref="FudgeEncodedStreamWriter"/> to use a different <see cref="Stream"/> for output.
        /// </summary>
        /// <param name="stream"></param>
        public void Reset(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            Reset(new FudgeBinaryWriter(stream));
        }

        /// <summary>
        /// Resets the <see cref="FudgeEncodedStreamWriter"/> to use a different <see cref="BinaryWriter"/> for output.
        /// </summary>
        /// <param name="writer"><see cref="BinaryWriter"/> to use for output.</param>
        public void Reset(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            this.outputWriter = writer;
        }

        #region IFudgeStreamWriter Members

        /// <inheritdoc/>
        public void StartMessage()
        {
            if (stack.Count > 0)
                throw new InvalidOperationException("Attempting to start message when already started.");

            memStream = new MemoryStream();
            memWriter = new FudgeBinaryWriter(memStream);
            taxonomy = null;
            if (TaxonomyId.HasValue && context.TaxonomyResolver != null)
            {
                taxonomy = context.TaxonomyResolver.ResolveTaxonomy(TaxonomyId.Value);
            }
            stack.Push(new State(0, 0, false, false, 0));
        }

        /// <inheritdoc/>
        public void StartSubMessage(string name, int? ordinal)
        {
            short? shortOrdinal = (short?)ordinal;
            NormalizeNameAndOrdinal(ref name, ref shortOrdinal, taxonomy);

            long prefixPos = memStream.Position;
            // We pretend the size is int.MinValue so it gets marked as four bytes
            int nWritten = WriteFieldHeader(memWriter, int.MaxValue, true, FudgeMsgFieldType.Instance.TypeId, shortOrdinal, name);
            int sizePos = (int)memStream.Position;
            sizePositions.Add(sizePos);                 // Remember where this is for when we write out to the real stream
            memWriter.Write(int.MaxValue);              // This will be four bytes

            var state = new State(prefixPos, sizePos, shortOrdinal.HasValue, name != null, nWritten);
            stack.Push(state);
        }

        /// <inheritdoc/>
        public void WriteField(string name, int? ordinal, FudgeFieldType type, object value)
        {
            if (type == FudgeMsgFieldType.Instance)
            {
                // Got a sub-message, so we need to stream that properly
                var msg = (IFudgeFieldContainer)value;
                StartSubMessage(name, ordinal);
                WriteFields(msg.GetAllFields());
                EndSubMessage();
            }
            else
            {
                int nWritten = WriteField(memWriter, type, value, (short?)ordinal, name, taxonomy);
                stack.Peek().FinalMessageContentsSize += nWritten;
            }
        }

        /// <inheritdoc/>
        public void WriteFields(IEnumerable<IFudgeField> fields)
        {
            foreach (var field in fields)
            {
                WriteField(field.Name, field.Ordinal, field.Type, field.Value);
            }
        }

        /// <inheritdoc/>
        public void EndSubMessage()
        {
            long currentPos = memStream.Position;
            var state = stack.Pop();
            int size = state.FinalMessageContentsSize;

            // Overwrite the prefix now we know the size
            int newFieldPrefix = FudgeFieldPrefixCodec.ComposeFieldPrefix(false, size, state.HasOrdinal, state.HasName);
            memStream.Position = state.PrefixPos;
            memWriter.Write((byte)newFieldPrefix);

            // Write the size into the four bytes we reserved earlier
            memStream.Position = state.SizePos;
            memWriter.Write(state.FinalMessageContentsSize);

            // Skip back to the end
            memStream.Position = currentPos;

            // Add this message's size to the parent message's total final size
            int sizeSize = FudgeFieldPrefixCodec.CalculateVarSizeSize(size);
            int totalMessageSize = state.FinalMessageContentsSize + state.HeaderSize + sizeSize;       // Although we took four bytes writing the size we won't when it's finally output
            stack.Peek().FinalMessageContentsSize += totalMessageSize;
        }

        /// <inheritdoc/>
        public void EndMessage()
        {
            if (stack.Count != 1)
            {
                throw new InvalidOperationException("Ending message before all sub-messages ended");
            }
            var state = stack.Pop();
            int totalMessageSize = state.FinalMessageContentsSize;
            WriteMsgEnvelopeHeader(outputWriter, TaxonomyId ?? 0, totalMessageSize, EnvelopeVersion);
            memStream.Position = 0;

            WriteMemStreamToOutput();

            // Tidy up
            sizePositions.Clear();
            memStream = null;
            memWriter = null;
        }

        #endregion

        private void WriteMemStreamToOutput()
        {
            if (memStream.Length > int.MaxValue)
            {
                throw new InvalidOperationException("Message size exceeded 2GB, this is not supported by Fudge.");
            }
            int memStreamLength = (int)memStream.Length;
            var reader = new FudgeBinaryReader(memStream);
            int currentPos = 0;
            for (int i = 0; i < sizePositions.Count; i++)
            {
                int sizePos = sizePositions[i];
                Debug.Assert(sizePos >= currentPos);

                // Copy up to the size
                currentPos += CopyStream(reader, outputWriter, sizePos - currentPos, copyBuffer);

                // Now read the size, which was written as four bytes
                int size = reader.ReadInt32();
                currentPos += 4;

                // And write it out at the correct length
                WriteVariableSize(outputWriter, size);
            }

            // Copy anything left
            CopyStream(reader, outputWriter, memStreamLength - currentPos, copyBuffer);
        }

        private static int CopyStream(BinaryReader source, BinaryWriter dest, int count, byte[] buffer)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int nRead = source.Read(buffer, 0, Math.Min(count - totalRead, buffer.Length));
                if (nRead == 0)
                    break;          // End of stream
                dest.Write(buffer, 0, nRead);

                totalRead += nRead;
            }
            return totalRead;
        }

        #region Main encoding stuff

        private static int WriteMsgFields(BinaryWriter bw, IFudgeFieldContainer container, IFudgeTaxonomy taxonomy) //throws IOException
        {
            int nWritten = 0;
            foreach (IFudgeField field in container.GetAllFields())
            {
                nWritten += WriteField(bw, field.Type, field.Value, field.Ordinal, field.Name, taxonomy);
            }
            return nWritten;
        }

        private static int WriteMsgEnvelopeHeader(BinaryWriter bw, int taxonomy, int messageSize, int version)// throws IOException
        {
            CheckOutputStream(bw);

            messageSize += 8;               // The size of the envelope header

            int nWritten = 0;

            bw.Write((byte)0); // Processing Directives
            nWritten += 1;
            bw.Write((byte)version);
            nWritten += 1;
            bw.Write((short)taxonomy);      // REVIEW 2009-10-04 t0rx -- Should this be ushort?
            nWritten += 2;
            bw.Write(messageSize);
            nWritten += 4;
            return nWritten;
        }

        private static int WriteField(BinaryWriter bw, FudgeFieldType type,
              object value, short? ordinal, string name,
              IFudgeTaxonomy taxonomy)// throws IOException
        {
            CheckOutputStream(bw);

            if (type == null)
            {
                throw new ArgumentNullException("Must provide the type of data encoded.");
            }
            if (value == null)
            {
                throw new ArgumentNullException("Must provide the value to encode.");
            }

            // First, normalize the name/ordinal bit
            NormalizeNameAndOrdinal(ref name, ref ordinal, taxonomy);

            // Reduce the value to minimal form
            value = type.Minimize(value, ref type);

            // Now do the field header
            int valueSize = type.IsVariableSize ? type.GetVariableSize(value, taxonomy) : type.FixedSize;
            int nWritten = WriteFieldHeader(bw, valueSize, type.IsVariableSize, type.TypeId, ordinal, name);

            // Finally the value
            if (value != null)
            {
                Debug.Assert(type != null);
                nWritten += WriteFieldValue(bw, type, value, valueSize, taxonomy);
            }
            return nWritten;
        }

        private static void NormalizeNameAndOrdinal(ref string name, ref short? ordinal, IFudgeTaxonomy taxonomy)
        {
            if ((taxonomy != null) && (name != null))
            {
                short? ordinalFromTaxonomy = taxonomy.GetFieldOrdinal(name);
                if (ordinalFromTaxonomy != null)
                {
                    if ((ordinal != null) && !object.Equals(ordinalFromTaxonomy, ordinal))
                    {
                        // In this case, we've been provided an ordinal, but it doesn't match the
                        // one from the taxonomy. We have to assume the user meant what they were doing,
                        // and not do anything.
                    }
                    else
                    {
                        ordinal = ordinalFromTaxonomy;
                        name = null;
                    }
                }
            }
        }

        private static int WriteFieldHeader(BinaryWriter bw, int valueSize, bool variableSize, int typeId, short? ordinal, string name)
        {
            int nWritten = 0;

            int fieldPrefix = FudgeFieldPrefixCodec.ComposeFieldPrefix(!variableSize, valueSize, (ordinal != null), (name != null));
            bw.Write((byte)fieldPrefix);
            nWritten++;
            bw.Write((byte)typeId);
            nWritten++;
            if (ordinal != null)
            {
                bw.Write(ordinal.Value);
                nWritten += 2;
            }
            if (name != null)
            {
                int utf8size = ModifiedUTF8Util.ModifiedUTF8Length(name);
                if (utf8size > 0xFF)
                {
                    throw new ArgumentOutOfRangeException("UTF-8 encoded field name cannot exceed 255 characters. Name \"" + name + "\" is " + utf8size + " bytes encoded.");
                }
                bw.Write((byte)utf8size);
                nWritten++;
                nWritten += ModifiedUTF8Util.WriteModifiedUTF8(name, bw);
            }

            return nWritten;
        }

        private static int WriteFieldValue(BinaryWriter bw, FudgeFieldType type, object value, int valueSize, IFudgeTaxonomy taxonomy) //throws IOException
        {
            // Note that we fast-path types for which at compile time we know how to handle
            // in an optimized way. This is because this particular method is known to
            // be a massive hot-spot for performance.
            int nWritten = 0;
            switch (type.TypeId)
            {
                case FudgeTypeDictionary.BOOLEAN_TYPE_ID:
                    bw.Write((bool)value);
                    nWritten = 1;
                    break;
                case FudgeTypeDictionary.SBYTE_TYPE_ID:
                    bw.Write((sbyte)value);
                    nWritten = 1;
                    break;
                case FudgeTypeDictionary.SHORT_TYPE_ID:
                    bw.Write((short)value);
                    nWritten = 2;
                    break;
                case FudgeTypeDictionary.INT_TYPE_ID:
                    bw.Write((int)value);
                    nWritten = 4;
                    break;
                case FudgeTypeDictionary.LONG_TYPE_ID:
                    bw.Write((long)value);
                    nWritten = 8;
                    break;
                case FudgeTypeDictionary.FLOAT_TYPE_ID:
                    bw.Write((float)value);
                    nWritten = 4;
                    break;
                case FudgeTypeDictionary.DOUBLE_TYPE_ID:
                    bw.Write((double)value);
                    nWritten = 8;
                    break;
            }

            if (nWritten == 0)
            {
                if (type.IsVariableSize)
                {
                    nWritten = WriteVariableSize(bw, valueSize);
                }
                else
                {
                    nWritten = type.FixedSize;
                }

                if (value is IFudgeFieldContainer)
                {
                    IFudgeFieldContainer subMsg = (IFudgeFieldContainer)value;
                    WriteMsgFields(bw, subMsg, taxonomy);
                }
                else
                {
                    type.WriteValue(bw, value);
                }
            }
            return nWritten;
        }

        private static void CheckOutputStream(BinaryWriter bw)
        {
            if (bw == null)
            {
                throw new ArgumentNullException("Must specify a BinaryWriter for processing.");
            }
        }

        private static int WriteVariableSize(BinaryWriter bw, int valueSize)
        {
            int nWritten;

            // This is correct. We read this using a .readUnsignedByte(), so we can go to
            // 255 here.
            if (valueSize <= 255)
            {
                bw.Write((byte)valueSize);
                nWritten = valueSize + 1;
            }
            else if (valueSize <= short.MaxValue)
            {
                bw.Write((short)valueSize);
                nWritten = valueSize + 2;
            }
            else
            {
                bw.Write((int)valueSize);
                nWritten = valueSize + 4;
            }
            return nWritten;
        }

        #endregion

        private class State
        {
            public readonly long PrefixPos;
            public readonly long SizePos;
            public readonly bool HasOrdinal;
            public readonly bool HasName;
            public readonly int HeaderSize;
            public int FinalMessageContentsSize = 0;

            public State(long prefixPos, long sizePos, bool hasOrdinal, bool hasName, int headerSize)
            {
                PrefixPos = prefixPos;
                SizePos = sizePos;
                HasOrdinal = hasOrdinal;
                HasName = hasName;
                HeaderSize = headerSize;
            }
        }
    }
}
