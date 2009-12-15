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
using Fudge.Taxon;
using System.IO;
using Fudge.Util;

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

        /// <summary>
        /// Gets or sets the <c>ITaxonomyResolver</c> for use within this context when encoding or decoding messages.
        /// </summary>
        public ITaxonomyResolver TaxonomyResolver
        {
            get { return taxonomyResolver; }
            set { taxonomyResolver = value; }
        }

        /// <summary>
        /// Gets or sets the <c>FudgeTypeDictionary</c> for use within this context when encoding or decoding messages.
        /// </summary>
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

        /// <summary>
        /// Creates a new, empty <c>FudgeMsg</c> object.
        /// </summary>
        /// <returns>the <c>FudgeMsg</c> created</returns>
        public FudgeMsg NewMessage()
        {
            return new FudgeMsg(TypeDictionary);
        }

        /// <summary>
        /// Encodes a <c>FudgeMsg</c> object to a <c>Stream</c> without any taxonomy reference.
        /// </summary>
        /// <param name="msg">The message to serialise</param>
        /// <param name="s">The stream to serialise to</param>
        public void Serialize(FudgeMsg msg, Stream s)
        {
            Serialize(msg, null, s);
        }

        /// <summary>
        /// Encodes a <c>FudgeMsg</c> object to a <c>Stream</c> with an optional taxonomy reference.
        /// If a taxonomy is supplied it may be used to optimize the output by writing ordinals instead
        /// of field names.
        /// </summary>
        /// <param name="msg">the <c>FudgeMessage</c> to write</param>
        /// <param name="taxonomyId">the identifier of the taxonomy to use. Specify <c>null</c> for no taxonomy</param>
        /// <param name="s">the <c>Stream</c> to write to</param>
        public void Serialize(FudgeMsg msg, short? taxonomyId, Stream s)
        {
            IFudgeTaxonomy taxonomy = null;
            if ((TaxonomyResolver != null) && (taxonomyId != null))
            {
                taxonomy = TaxonomyResolver.ResolveTaxonomy(taxonomyId.Value);
            }
            BinaryWriter bw = new FudgeBinaryWriter(s);
            try
            {
                FudgeStreamEncoder.WriteMsg(bw, new FudgeMsgEnvelope(msg), TypeDictionary, taxonomy, taxonomyId ?? 0);
            }
            catch (IOException e)
            {
                throw new FudgeRuntimeException("Unable to write Fudge message to OutputStream", e);
            }
        }

        /// <summary>
        /// Returns the Fudge encoded form of a <c>FudgeMsg</c> as a <c>byte</c> array without a taxonomy reference.
        /// </summary>
        /// <param name="msg">the <c>FudgeMsg</c> to encode</param>
        /// <returns>an array containing the encoded message</returns>
        public byte[] ToByteArray(FudgeMsg msg)
        {
            MemoryStream stream = new MemoryStream();
            Serialize(msg, stream);
            return stream.ToArray();
        }

        // TODO 2009-12-11 Andrew -- should we have a toByteArray that takes a taxonomy too?

        // TODO 2009-12-11 Andrew -- should we have a Serialize that takes the envelope as there's no way to control the version field otherwise?
 
        /// <summary>
        /// Decodes a <c>FudgeMsg</c> from a <c>Stream</c>.
        /// </summary>
        /// <param name="s">the <c>Stream</c> to read encoded data from</param>
        /// <returns>the next <c>FudgeMsgEnvelope</c> encoded on the stream</returns>
        public FudgeMsgEnvelope Deserialize(Stream s)
        {
            BinaryReader br = new FudgeBinaryReader(s);
            FudgeMsgEnvelope envelope;
            try
            {
                envelope = FudgeStreamDecoder.ReadMsg(br, TypeDictionary, TaxonomyResolver);
            }
            catch (IOException e)
            {
                throw new FudgeRuntimeException("Unable to deserialize FudgeMsg from input stream", e);
            }
            return envelope;
        }

        /// <summary>
        /// Decodes a <c>FudgeMsg</c> from a <c>byte</c> array. If the array is larger than the Fudge envelope, any additional data is ignored.
        /// </summary>
        /// <param name="bytes">an array containing the envelope encoded <c>FudgeMsg</c></param>
        /// <returns>the decoded <c>FudgeMsgEnvelope</c></returns>
        public FudgeMsgEnvelope Deserialize(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes, false);
            return Deserialize(stream);
        }

        // TODO 2009-12-11 Andrew -- should we have a version that takes an offset so that arrays with more than one envelope can be processed?

    }
}
