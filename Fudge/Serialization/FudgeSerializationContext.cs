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

namespace Fudge.Serialization
{
    public class FudgeSerializationContext : IFudgeSerializationContext
    {
        private readonly Queue<object> encodeQueue = new Queue<object>();
        private readonly SerializationMessage message;
        private readonly Dictionary<object, int> idMap;     // Tracks IDs of objects that have already been serialised (or are in the process)
        private readonly SerializationTypeMap typeMap;

        public FudgeSerializationContext(FudgeSerializer serializer, SerializationMessage message)
        {
            this.message = message;
            this.idMap = new Dictionary<object, int>();     // TODO t0rx 2009-10-18 -- Worry about HashCode and Equals implementations
            this.typeMap = serializer.TypeMap;
        }

        public void QueueObject(object obj)
        {
            encodeQueue.Enqueue(obj);
        }

        public object PopQueuedObject()
        {
            if (encodeQueue.Count == 0)
            {
                return null;
            }

            return encodeQueue.Dequeue();
        }

        public int RegisterObject(object obj)
        {
            int id;
            if (idMap.TryGetValue(obj, out id))
            {
                // Already registered
                return id;
            }
            id = idMap.Count;
            idMap.Add(obj, id);
            return id;
        }

        #region IFudgeSerializationContext Members

        public FudgeMsg AsSubMsg(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var msg = new FudgeMsg();

            SerializeIntoMessage(obj, msg);
            return msg;
        }

        public int AsRef(object obj)
        {
            if (obj == null)
            {
                // TODO torx 2009-10-18 -- Handle null references
            }

            int id;
            if (idMap.TryGetValue(obj, out id))
            {
                // Already got it
                return id;
            }

            // New object
            id = RegisterObject(obj);
            QueueObject(obj);
            return id;
        }

        #endregion

        public void SerializeIntoMessage(object obj, FudgeMsg msg)
        {
            var surrogate = typeMap.GetSurrogate(obj.GetType());
            if (surrogate == null)
            {
                // Unknown type
                throw new ArgumentOutOfRangeException("Type \"" + obj.GetType().FullName + "\" not registered, cannot serialize");
            }

            surrogate.Serialize(obj, msg, this);
        }
    }
}
