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

namespace OpenGamma.Fudge.Serialization
{
    public class FudgeSerializer
    {
        private readonly SerializationTypeMap typeMap;
        public const string TypeIdFieldName = "__typeId";

        public FudgeSerializer(SerializationTypeMap typeMap)
        {
            if (typeMap == null)
                throw new ArgumentNullException("typeMap");

            this.typeMap = typeMap;
        }

        public FudgeMsg Serialize(object graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException("graph");
            }

            var message = new SerializationMessage(typeMap);
            var context = new FudgeSerializationContext(this, message);

            context.QueueObject(graph);

            object nextObj;
            while ((nextObj = context.PopQueuedObject()) != null)
            {
                context.RegisterObject(nextObj);     // Must register before serialising in case of circular references
                SerializeObject(nextObj, context, message);
            }

            return message.ToMessage();
        }

        public object Deserialize(FudgeMsg fudgeMsg)
        {
            var sm = new SerializationMessage(fudgeMsg, typeMap);
            var context = new FudgeDeserializationContext(this, sm);

            // Our result is simply the first object in the message
            var result = context.FromRef<object>(0);

            return result;
        }

        public SerializationTypeMap TypeMap
        {
            get { return typeMap; }
        }

        private void SerializeObject(object obj, FudgeSerializationContext context, SerializationMessage message)
        {
            var msg = new FudgeMsg();

            // Add in the type ID for when we deserialise
            int typeId = typeMap.GetTypeId(obj.GetType());
            msg.Add(TypeIdFieldName, typeId);

            context.SerializeIntoMessage(obj, msg);
            message.AddObject(msg);
        }
    }
}
