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
using System.Linq;
using System.Text;
using Fudge.Encodings;
using Fudge.Types;

namespace Fudge.Serialization
{
    public class FudgeSerializer
    {
        private readonly FudgeContext context;
        private readonly SerializationTypeMap typeMap;

        public const int TypeIdFieldOrdinal = 0;

        public FudgeSerializer(FudgeContext context)
            : this(context, null)
        {
        }

        public FudgeSerializer(FudgeContext context, SerializationTypeMap typeMap)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (typeMap == null)
            {
                // TODO 2010-02-02 t0rx -- Have serialization type map as context property?
                typeMap = new SerializationTypeMap(context);
            }

            this.context = context;
            this.typeMap = typeMap;
        }

        public void Serialize(IFudgeStreamWriter writer, object graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException("graph");
            }

            // Delegate to FudgeSerializationContext to do the work
            var serializationContext = new FudgeSerializationContext(context, typeMap, writer);
            serializationContext.SerializeGraph(writer, graph);
        }

        public IList<FudgeMsg> SerializeToMsgs(object graph)
        {
            var writer = new FudgeMsgStreamWriter(context);
            Serialize(writer, graph);
            return writer.PeekAllMessages();
        }

        public object Deserialize(IFudgeStreamReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Delegate to FudgeDeserializer to do the work
            var deserializer = new FudgeDeserializationContext(context, typeMap, reader);
            return deserializer.DeserializeGraph();
        }

        public object Deserialize(IEnumerable<FudgeMsg> msgs)
        {
            var reader = new FudgeMsgStreamReader(context, msgs);
            return Deserialize(reader);
        }
    }
}
