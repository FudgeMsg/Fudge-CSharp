/*
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
using Fudge.Taxon;
using System.IO;
using Fudge.Util;
using Fudge.Encodings;

namespace Fudge
{
    /// <summary>
    /// The primary entry-point for code to interact with the rest of the fudge system.
    /// For performance reasons, there are many options that are passed around as parameters
    /// inside static methods for encoding and decoding, and many lightweight objects that
    /// ideally don't know of their configuration context.
    /// However, in a large application, it is often desirable to collect all configuration
    /// parameters in one location and inject options into it.
    /// <p/>
    /// <c>FudgeContext</c> allows application developers to have a single location
    /// to inject dependent parameters and instances, and make them available through
    /// simple method invocations. in addition, because it wraps all checked exceptions
    /// into instances of <see cref="FudgeRuntimeException"/>, it is the ideal way to use
    /// the fudge encoding system from within spring applications.
    /// <p/>
    /// While most applications will have a single instance of <c>FudgeContext</c>,
    /// some applications will have one instance per unit of encoding/decoding parameters.
    /// for example, if an application is consuming data from two messaging feeds, each
    /// of which reuses the same taxonomy id to represent a different
    /// <see cref="IFudgeTaxonomy"/>, it would configure two different instances of
    /// <c>FudgeContext</c>, one per feed.  
    /// </summary>
    public class FudgeContext
    {
        private FudgeTypeDictionary typeDictionary = new FudgeTypeDictionary();
        private ITaxonomyResolver taxonomyResolver;
        private FudgeStreamParser parser;

        public FudgeContext()
        {
            parser = new FudgeStreamParser(this);
        }

        public ITaxonomyResolver TaxonomyResolver
        {
            get { return taxonomyResolver; }
            set { taxonomyResolver = value; }
        }

        public FudgeTypeDictionary TypeDictionary
        {
            get { return typeDictionary; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", "Every Fudge context must have a type dictionary.");
                }
                typeDictionary = value;
            }
        }

        public FudgeMsg NewMessage()
        {
            return new FudgeMsg(this);
        }

        public FudgeMsg NewMessage(params IFudgeField[] fields)
        {
            return new FudgeMsg(this, fields);
        }

        public void Serialize(FudgeMsg msg, Stream s)
        {
            Serialize(msg, null, new FudgeBinaryWriter(s));
        }

        public void Serialize(FudgeMsg msg, BinaryWriter bw)
        {
            Serialize(msg, null, bw);
        }

        public void Serialize(FudgeMsg msg, short? taxonomyId, Stream s)
        {
            Serialize(msg, taxonomyId, new FudgeBinaryWriter(s));
        }

        public void Serialize(FudgeMsg msg, short? taxonomyId, BinaryWriter bw)
        {
            try
            {
                var writer = new FudgeEncodedStreamWriter(this);            // TODO t0rx 2009-11-12 -- Fudge-Java gets this from an allocated queue
                writer.TaxonomyId = taxonomyId;
                writer.Reset(bw);
                writer.StartSubMessage(null, null);
                writer.WriteFields(msg.GetAllFields());
                writer.EndSubMessage();
            }
            catch (IOException e)
            {
                throw new FudgeRuntimeException("Unable to write Fudge message to OutputStream", e);
            }
        }

        public byte[] ToByteArray(FudgeMsg msg)
        {
            MemoryStream stream = new MemoryStream();
            Serialize(msg, stream);
            return stream.ToArray();
        }  

        public FudgeMsgEnvelope Deserialize(Stream s)
        {
            BinaryReader br = new FudgeBinaryReader(s);
            FudgeMsgEnvelope envelope;
            try
            {
                envelope = parser.Parse(br);
            }
            catch (IOException e)
            {
                throw new FudgeRuntimeException("Unable to deserialize FudgeMsg from input stream", e);
            }
            return envelope;
        }

        public FudgeMsgEnvelope Deserialize(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes, false);
            return Deserialize(stream);
        }
  

    }
}
