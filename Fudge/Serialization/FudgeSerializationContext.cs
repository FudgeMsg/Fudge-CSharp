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
using Fudge.Types;
using Fudge.Encodings;

namespace Fudge.Serialization
{
    internal class FudgeSerializationContext : IFudgeSerializer
    {
        private readonly FudgeContext context;
        private readonly IFudgeStreamWriter writer;
        private readonly Queue<object> encodeQueue = new Queue<object>();
        private readonly Dictionary<object, int> idMap;     // Tracks IDs of objects that have already been serialised (or are in the process)
        private readonly SerializationTypeMap typeMap;

        public FudgeSerializationContext(FudgeContext context, SerializationTypeMap typeMap, IFudgeStreamWriter writer)
        {
            this.context = context;
            this.writer = writer;
            this.idMap = new Dictionary<object, int>();     // TODO t0rx 2009-10-18 -- Worry about HashCode and Equals implementations
            this.typeMap = typeMap;
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

        public void SerializeContents(object obj)
        {
            var surrogateFactory = typeMap.GetSurrogateFactory(obj.GetType());
            if (surrogateFactory == null)
            {
                // Unknown type
                throw new ArgumentOutOfRangeException("Type \"" + obj.GetType().FullName + "\" not registered, cannot serialize");
            }
            var surrogate = surrogateFactory(context);

            surrogate.Serialize(obj, this);
        }

        public void SerializeGraph(IFudgeStreamWriter writer, object graph)
        {
            // Write the header message
            var header = new SerializationHeader(typeMap);
            writer.WriteMsg(header.ToMessage());

            // Write out all the objects
            QueueObject(graph);
            ProcessSerializationQueue(writer);

            // Write the end marker
            writer.WriteMsg(context.NewMessage());
        }

        public void ProcessSerializationQueue(IFudgeStreamWriter writer)
        {
            object nextObj;
            while ((nextObj = PopQueuedObject()) != null)
            {
                RegisterObject(nextObj);     // Must register before serialising in case of circular references
                SerializeObject(nextObj, writer);
            }
        }

        private void SerializeObject(object obj, IFudgeStreamWriter writer)
        {
            writer.StartMessage();

            // Add in the type ID for when we deserialise
            int typeId = typeMap.GetTypeId(obj.GetType());
            writer.WriteField(null, FudgeSerializer.TypeIdFieldOrdinal, PrimitiveFieldTypes.IntType, typeId);

            SerializeContents(obj);

            writer.EndMessage();
        }

        #region IFudgeSerializer Members

        public void Write(string fieldName, int? ordinal, object value)
        {
            FudgeFieldType type = context.TypeHandler.DetermineTypeFromValue(value);
            if (type == null)
            {
                throw new FudgeRuntimeException("Could not write field (name '" + fieldName + "') of type " + value.GetType());
            }
            writer.WriteField(fieldName, ordinal, type, value);
        }

        public void WriteSubMsg(string fieldName, int? ordinal, object obj)
        {
            if (obj != null)
            {
                writer.StartSubMessage(fieldName, ordinal);
                SerializeContents(obj);
                writer.EndSubMessage();
            }
        }

        public void WriteRef(string fieldName, int? ordinal, object obj)
        {
            if (obj == null)
            {
                // TODO torx 2009-10-18 -- Handle null references
            }

            Write(fieldName, ordinal, GetRefId(obj));
        }

        #endregion

        private int GetRefId(object obj)
        {
            int id;
            if (!idMap.TryGetValue(obj, out id))
            {
                // New object
                id = RegisterObject(obj);
                QueueObject(obj);
            }

            return id;
        }
    }

    /*
        private readonly SerializationMessage message;
        private readonly Dictionary<object, int> idMap;     // Tracks IDs of objects that have already been serialised (or are in the process)
        private readonly SerializationTypeMap typeMap;

        public FudgeSerializationContext(FudgeSerializer serializer, SerializationMessage message)
        {
            this.message = message;
            this.idMap = new Dictionary<object, int>();     // TODO t0rx 2009-10-18 -- Worry about HashCode and Equals implementations
            this.typeMap = serializer.TypeMap;
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
     */
}
