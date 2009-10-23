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

        public FudgeMsgEnvelope()
            : this(new FudgeMsg())
        {
        }

        public FudgeMsgEnvelope(FudgeMsg msg)
            : this(msg, 0)
        {
        }

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

        public FudgeMsg Message
        {
            get { return message; }
        }

        public int Version
        {
            get { return version; }
        }

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
