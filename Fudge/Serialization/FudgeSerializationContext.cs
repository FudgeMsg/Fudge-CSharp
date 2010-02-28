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
    /// <summary>
    /// Provides an implementation of <see cref="IFudgeSerializer"/> used by the <see cref="FudgeSerializer"/>.
    /// </summary>
    /// <remarks>
    /// You should not need to use this class directly.
    /// </remarks>
    internal class FudgeSerializationContext : IFudgeSerializer
    {
        private readonly FudgeContext context;
        private readonly IFudgeStreamWriter writer;
        private readonly Queue<object> encodeQueue = new Queue<object>();
        private readonly Dictionary<object, int> idMap;     // Tracks IDs of objects that have already been serialised (or are in the process)
        private readonly Dictionary<Type, int> lastTypes = new Dictionary<Type, int>();     // Tracks the last object of a given type
        private readonly SerializationTypeMap typeMap;
        private readonly IFudgeTypeMappingStrategy typeMappingStrategy;
        private readonly List<object> inlineStack = new List<object>();                     // Used to check for cycles in inlined messages - see comments in CheckForInlineCycles
        private int currentId = 0;

        public FudgeSerializationContext(FudgeContext context, SerializationTypeMap typeMap, IFudgeStreamWriter writer, IFudgeTypeMappingStrategy typeMappingStrategy)
        {
            this.context = context;
            this.writer = writer;
            this.idMap = new Dictionary<object, int>();     // TODO 2009-10-18 t0rx -- Worry about HashCode and Equals implementations
            this.typeMap = typeMap;
            this.typeMappingStrategy = typeMappingStrategy;
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
            CheckForInlineCycles(obj);

            inlineStack.Add(obj);

            var surrogateFactory = typeMap.GetSurrogateFactory(obj.GetType());
            if (surrogateFactory == null)
            {
                // Unknown type
                throw new ArgumentOutOfRangeException("Type \"" + obj.GetType().FullName + "\" not registered, cannot serialize");
            }
            var surrogate = surrogateFactory(context);

            surrogate.Serialize(obj, this);

            inlineStack.RemoveAt(inlineStack.Count - 1);
        }

        private void CheckForInlineCycles(object obj)
        {
            // We're using a List rather than a stack because enumerating a stack is much slower
            for (int i = inlineStack.Count - 1; i >= 0; i--)
            {
                if (obj == inlineStack[i])
                    throw new FudgeRuntimeException("Cycle detected in inlined objects at object of type " + obj.GetType());
            }
        }

        public void SerializeGraph(IFudgeStreamWriter writer, object graph)
        {
            // Write the header message
            // TODO 2010-02-28 t0rx -- Get rid of serialization header altogether?
            //var header = new SerializationHeader(typeMap);
            //writer.WriteMsg(header.ToMessage());

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
                currentId++;
            }
        }

        private void SerializeObject(object obj, IFudgeStreamWriter writer)
        {
            int id = currentId;

            writer.StartMessage();

            // Add in the type ID for when we deserialise
            WriteTypeInformation(obj, id, writer);

            SerializeContents(obj);

            writer.EndMessage();

            // Update where we last saw the type
            lastTypes[obj.GetType()] = id;
        }

        private void WriteTypeInformation(object obj, int id, IFudgeStreamWriter writer)
        {
            Type type = obj.GetType();
            int lastSeen;
            if (lastTypes.TryGetValue(type, out lastSeen))
            {
                // Already had something of this type
                int offset = lastSeen - id;
                writer.WriteField(null, FudgeSerializer.TypeIdFieldOrdinal, PrimitiveFieldTypes.IntType, offset);
            }
            else
            {
                // Not seen before, so write out with base types
                for (Type currentType = type; currentType != typeof(object); currentType = currentType.BaseType)
                {
                    string typeName = typeMappingStrategy.GetName(currentType);
                    writer.WriteField(null, FudgeSerializer.TypeIdFieldOrdinal, StringFieldType.Instance, typeName);
                }
            }
        }

        #region IFudgeSerializer Members

        /// <inheritdoc/>
        public FudgeContext Context
        {
            get { return context; }
        }

        /// <inheritdoc/>
        public void Write(string fieldName, int? ordinal, object value)
        {
            FudgeFieldType type = context.TypeHandler.DetermineTypeFromValue(value);
            if (type == null)
            {
                throw new FudgeRuntimeException("Could not write field (name '" + fieldName + "') of type " + value.GetType());
            }
            writer.WriteField(fieldName, ordinal, type, value);
        }

        /// <inheritdoc/>
        public void WriteSubMsg(string fieldName, int? ordinal, object obj)
        {
            if (obj != null)
            {
                writer.StartSubMessage(fieldName, ordinal);
                SerializeContents(obj);
                writer.EndSubMessage();
            }
        }

        /// <inheritdoc/>
        public void WriteRef(string fieldName, int? ordinal, object obj)
        {
            if (obj == null)
            {
                // TODO 2009-10-18 torx -- Handle null references
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
                // TODO 2009-10-18 torx -- Handle null references
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
