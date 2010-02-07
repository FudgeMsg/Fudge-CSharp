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
using System.Runtime.Serialization;
using Fudge.Types;
using Fudge.Encodings;
using System.Diagnostics;

namespace Fudge.Serialization
{
    public class FudgeDeserializationContext : IFudgeDeserializer
    {
        private readonly FudgeContext context;
        private readonly SerializationTypeMap typeMap;
        private readonly IFudgeStreamReader reader;
        private readonly FudgeMsgStreamWriter msgWriter;
        private readonly FudgeStreamPipe pipe;
        private readonly List<object> reffedObjects;        // Holds objects that have been (or are being deserialised) that are references
        private readonly List<FudgeMsg> unprocessedObjects;
        private bool reachedEnd;
        private readonly Stack<State> stack;

        public FudgeDeserializationContext(FudgeContext context, SerializationTypeMap typeMap, IFudgeStreamReader reader)
        {
            this.context = context;
            this.typeMap = typeMap;
            this.reader = reader;
            this.msgWriter = new FudgeMsgStreamWriter(context);
            this.pipe = new FudgeStreamPipe(reader, msgWriter);
            this.reffedObjects = new List<object>();
            this.unprocessedObjects = new List<FudgeMsg>();
            this.reachedEnd = false;
            this.stack = new Stack<State>();
        }

        public object DeserializeGraph()
        {
            var header = new SerializationHeader(ReadNextMessage(), typeMap);

            // We simply return the first object
            object result = GetFromRef(0);

            // Ensure we've exhausted the stream
            ReadUpToEnd();

            return result;
        }

        private object GetFromRef(int? refId)
        {
            if (refId == null)
            {
                return null;
            }

            int index = refId.Value;

            while (unprocessedObjects.Count <= index)
            {
                FudgeMsg msg = ReadNextMessage();
                if (msg == null)
                {
                    Debug.Assert(reachedEnd);

                    // TODO 2010-01-24 t0rx -- Do we need a separate FudgeSerializatonException?
                    throw new FudgeRuntimeException("Attempt to deserialize object reference with ID " + refId + " but only " + unprocessedObjects.Count + " objects in stream.");
                }

                unprocessedObjects.Add(msg);
                reffedObjects.Add(null);            // Placeholder
            }
            Debug.Assert(unprocessedObjects.Count == reffedObjects.Count);

            object result = reffedObjects[index];
            if (result == null)
            {
                // Not processed yet
                result = ProcessObject(index, null);
            }
            return result;
        }

        private object ProcessObject(int index, Type hintType)
        {
            Debug.Assert(reffedObjects[index] == null);
            Debug.Assert(unprocessedObjects[index] != null);

            var message = unprocessedObjects[index];
            unprocessedObjects[index] = null;                           // Just making sure we don't try to process the same one twice
            int typeId;
            object result = ProcessObject(index, hintType, message, out typeId);

            // Make sure the object was registered by the surrogate
            if (reffedObjects[index] == null)
            {
                throw new SerializationException("Object not registered during deserialization with type " + typeMap.GetTypeName(typeId));
            }
            Debug.Assert(result == reffedObjects[index]);

            return result;
        }

        private object ProcessObject(int? refId, Type hintType, FudgeMsg message, out int typeId)
        {
            Type objectType;
            string typeName;
            IFudgeField typeField = message.GetByOrdinal(FudgeSerializer.TypeIdFieldOrdinal);
            if (typeField == null)
            {
                if (hintType == null)
                {
                    throw new FudgeRuntimeException("Serialized object has no type ID");
                }

                objectType = hintType;
            }
            else if (typeField.Type == StringFieldType.Instance)
            {
                // It's the first time we've seen this type in this graph, so it contains the type names
                typeName = (string)typeField.Value;
                // TODO 2010-02-07 t0rx -- Allow type name strategy to be plugged in
                objectType = Type.GetType(typeName);
                if (objectType == null)
                {
                    // TODO 2010-02-07 t0rx -- Try ancestors
                    throw new FudgeRuntimeException("Could not find type \"" + typeName + "\" to deserialize.");
                }
            }
            else
            {
                if (refId == null)
                {
                    throw new FudgeRuntimeException("Cannot use relative type IDs in sub-messages.");
                }
                int previousObjId = refId.Value + Convert.ToInt32(typeField.Value);

                if (previousObjId < 0 || previousObjId >= refId.Value)
                {
                    throw new FudgeRuntimeException("Illegal relative type ID in sub-message: " + typeField.Value);
                }

                objectType = reffedObjects[previousObjId].GetType();
            }

            typeId = typeMap.GetTypeId(objectType);
            typeName = typeMap.GetTypeName(typeId);
            var surrogateFactory = typeMap.GetSurrogateFactory(typeId);
            if (surrogateFactory == null)
            {
                throw new SerializationException("Type ID " + typeId + " not registered with serialization type map");
            }
            var surrogate = surrogateFactory(context);
            if (surrogate == null)
            {
                throw new SerializationException("Surrogate factory for type " + typeName + " returned null surrogate.");
            }
            int typeVersion = typeMap.GetTypeVersion(typeId);

            var state = new State(message, refId, typeVersion, typeName);

            stack.Push(state);
            object surrogateState = surrogate.BeginDeserialize(this, typeVersion);

            IFudgeField field;
            while ((field = state.NextField()) != null)
            {
                if (!surrogate.DeserializeField(this, field, typeVersion, surrogateState))
                    state.AddToUnused(field);
            }

            var result = surrogate.EndDeserialize(this, typeVersion, surrogateState);

            stack.Pop();
            return result;
        }


        private FudgeMsg ReadNextMessage()
        {
            if (reachedEnd)
                return null;

            pipe.ProcessOne();
            var msg = msgWriter.DequeueMessage();

            if (msg.GetNumFields() == 0)
            {
                reachedEnd = true;
                msg = null;
            }

            return msg;
        }

        private void ReadUpToEnd()
        {
            while (!reachedEnd)
            {
                ReadNextMessage();
            }
        }

        #region IFudgeDeserializer Members

        /// <inheritdoc/>
        public IFudgeFieldContainer GetUnreadFields()
        {
            var state = stack.Peek();

            return state.ConvertUnreadToMessage();
        }

        /// <inheritdoc/>
        public T FromField<T>(IFudgeField field) where T : class
        {
            if (field == null)
                return null;

            if (field.Type == FudgeMsgFieldType.Instance)
            {
                // SubMsg
                var subMsg = (FudgeMsg)field.Value;
                int typeId;
                return (T)ProcessObject(null, typeof(T), subMsg, out typeId);
            }
            else
            {
                int refId = Convert.ToInt32(field.Value);
                return (T)GetFromRef(refId);
            }
        }

        /// <inheritdoc/>
        public void Register(object obj)
        {
            State state = stack.Peek();
            if (state.RefId == null)
                return;                     // Don't care

            int index = state.RefId.Value;
            Debug.Assert(reffedObjects.Count > index);

            if (reffedObjects[index] != null)
            {
                throw new SerializationException("Attempt to register same deserialized object twice for type " + state.TypeName + " refID=" + index);
            }

            reffedObjects[index] = obj;
        }

        #endregion

        private class State
        {
            private readonly FudgeContext context;
            private readonly int? refId;
            private readonly Queue<IFudgeField> fields;
            private readonly int typeVersion;
            private readonly List<IFudgeField> unusedFields;
            private readonly string typeName;

            public State(FudgeMsg msg, int? refId, int typeVersion, string typeName)
            {
                this.context = msg.Context;
                this.fields = new Queue<IFudgeField>(msg.GetAllFields());
                this.refId = refId;
                this.typeVersion = typeVersion;
                this.unusedFields = new List<IFudgeField>();
                this.typeName = typeName;
            }

            public int? RefId
            {
                get { return refId; }
            }

            public string TypeName
            {
                get { return typeName; }
            }

            public IFudgeField NextField()
            {
                while (true)
                {
                    if (fields.Count == 0)
                        return null;

                    var field = fields.Dequeue();
                    if (field.Ordinal != FudgeSerializer.TypeIdFieldOrdinal)                    // Filter out the type ID
                        return field;
                }
            }

            public void AddToUnused(IFudgeField field)
            {
                unusedFields.Add(field);
            }

            internal IFudgeFieldContainer ConvertUnreadToMessage()
            {
                var result = context.NewMessage();
                result.Add(unusedFields);

                IFudgeField field;
                while ((field = NextField()) != null)
                    result.Add(field);

                return result;
            }
        }
    }
}
