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

namespace Fudge
{
    /// <summary>
    /// Wraps a <see cref="FudgeMsg"/> for the purpose of encoding the envelope header.
    /// This is the object which is encoded for a top-level fudge message; sub-messages don't
    /// contain a separate envelope.
    /// </summary>
    public class FudgeMsgEnvelope : FudgeEncodingObject
    {
        private readonly FudgeMsg message;
        private readonly int version;

        /// <summary>
        /// Constructs a new envelope containing an emtpy message.
        /// </summary>
        public FudgeMsgEnvelope()
            : this(new FudgeMsg())
        {
        }

        // TODO 2009-12-14 Andrew -- lose the constructor above; we should at least pass the context and use that to construct the inner message

        /// <summary>
        /// Creates a new <c>FudgeMsgEnvelope</c> around an existing <c>FudgeMsg</c>.
        /// </summary>
        /// <param name="msg">message contained within the envelope</param>
        public FudgeMsgEnvelope(FudgeMsg msg)
            : this(msg, 0)
        {
        }

        /// <summary>
        /// Creates a new <c>FudgeMsgEnvelope</c> around an existing <c>FudgeMsg</c> with a specific encoding schema version. The default
        /// schema version is 0.
        /// </summary>
        /// <param name="message">message contained within the envelope</param>
        /// <param name="version">schema version, 0 to 255</param>
        public FudgeMsgEnvelope(FudgeMsg message, int version)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message", "Must specify a message to wrap.");
            }
            if ((version < 0) || (version > 255))
            {
                throw new ArgumentOutOfRangeException("version", "Provided version " + version + " which doesn't fit within one byte.");
            }
            this.message = message;
            this.version = version;
        }

        /// <summary>
        /// Gets the message contained within this envelope.
        /// </summary>
        public FudgeMsg Message
        {
            get { return message; }
        }

        /// <summary>
        /// Gets the schema version of the envelope.
        /// </summary>
        public int Version
        {
            get { return version; }
        }

        /// <summary>
        /// Calculates the total size of the envelope. This is the encoded size of the message within the envelope plus the 8 byte envelope header.
        /// </summary>
        /// <param name="taxonomy">optional taxonomy to encode the message with</param>
        /// <returns>total size in bytes</returns>
        public override int ComputeSize(IFudgeTaxonomy taxonomy)
        {
            int size = 0;
            // Message envelope header
            size += 8;
            size += message.GetSize(taxonomy);
            return size;
        }
    }
}
