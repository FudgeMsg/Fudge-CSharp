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
using Fudge.Util;

namespace Fudge.Serialization
{
    /// <summary>
    /// Provides an implementation of <see cref="IFudgeDeserializer"/> used by the <see cref="FudgeSerializer"/>.
    /// </summary>
    /// <remarks>
    /// You should not need to use this class directly.
    /// </remarks>
    internal class FudgeDeserializationContext : IFudgeDeserializer
    {
        private readonly FudgeContext context;
        private readonly SerializationTypeMap typeMap;
        private readonly IFudgeStreamReader reader;
        private readonly FudgeMsgStreamWriter msgWriter;
        private readonly FudgeStreamPipe pipe;
        private readonly List<MsgAndObj> objectList;        // Holds messages and the objects they've deserialized into (for use with references)
        private readonly Dictionary<IFudgeFieldContainer, int> msgToIndexMap = new Dictionary<IFudgeFieldContainer, int>();
        private readonly Stack<State> stack;
        private readonly IFudgeTypeMappingStrategy typeMappingStrategy;

        public FudgeDeserializationContext(FudgeContext context, SerializationTypeMap typeMap, IFudgeStreamReader reader, IFudgeTypeMappingStrategy typeMappingStrategy)
        {
            this.context = context;
            this.typeMap = typeMap;
            this.reader = reader;
            this.msgWriter = new FudgeMsgStreamWriter(context);
            this.pipe = new FudgeStreamPipe(reader, msgWriter);
            this.objectList = new List<MsgAndObj>();
            this.stack = new Stack<State>();
            this.typeMappingStrategy = typeMappingStrategy;
        }

        public object DeserializeGraph()
        {
            // We simply return the first object
            var msg = ReadNextMessage();

            WalkMessage(msg);

            object result = GetFromRef(0);

            return result;
        }

        private void WalkMessage(FudgeMsg msg)
        {
            // TODO: Should do this as it's streaming in rather than separately
            MsgAndObj msgAndObj = new MsgAndObj();
            msgAndObj.Msg = msg;
            int index = objectList.Count;
            objectList.Add(msgAndObj);
            msgToIndexMap[msg] = index;
            foreach (var field in msg)
            {
                if (field.Type == FudgeMsgFieldType.Instance)
                {
                    WalkMessage((FudgeMsg)field.Value);
                }
            }
        }

        private object GetFromRef(int? refId)
        {
            if (refId == null)
            {
                return null;
            }

            int index = refId.Value;

            if (index < 0 || index >= objectList.Count)
            {
                throw new FudgeRuntimeException("Attempt to deserialize object reference with ID " + refId + " but only " + objectList.Count + " objects in stream so far.");
            }

            var msgAndObj = objectList[index];
            if (msgAndObj.Obj == null)
            {
                // Not processed yet
                ProcessObject(index, null);

                Debug.Assert(msgAndObj.Obj != null);
            }
            return msgAndObj.Obj;
        }

        private object ProcessObject(int index, Type hintType)
        {
            Debug.Assert(objectList[index].Obj == null);
            Debug.Assert(objectList[index].Msg != null);

            var message = objectList[index].Msg;
            objectList[index].Msg = null;                           // Just making sure we don't try to process the same one twice
            int typeId;
            object result = ProcessObject(index, hintType, message, out typeId);

            // Make sure the object was registered by the surrogate
            if (objectList[index].Obj == null)
            {
                throw new SerializationException("Object not registered during deserialization with type " + typeMap.GetTypeName(typeId));
            }
            Debug.Assert(result == objectList[index].Obj);

            return result;
        }

        private object ProcessObject(int refId, Type hintType, FudgeMsg message, out int typeId)
        {
            Type objectType = null;
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
                objectType = typeMappingStrategy.GetType(typeName);
                if (objectType == null)
                {
                    var typeNames = message.GetAllValues<string>(FudgeSerializer.TypeIdFieldOrdinal);
                    for (int i = 1; i < typeNames.Count; i++)       // 1 because we've already tried the first
                    {
                        objectType = typeMappingStrategy.GetType(typeNames[i]);
                        if (objectType != null)
                            break;                   // Found it
                    }
                }
            }
            else
            {
                int previousObjId = refId + Convert.ToInt32(typeField.Value);

                if (previousObjId < 0 || previousObjId >= refId)
                {
                    throw new FudgeRuntimeException("Illegal relative type ID in sub-message: " + typeField.Value);
                }

                object previous = GetFromRef(previousObjId);
                objectType = previous.GetType();
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

            var state = new State(message, refId, typeName);

            stack.Push(state);

            object result = surrogate.Deserialize(message, this);

            stack.Pop();
            return result;
        }

        private FudgeMsg ReadNextMessage()
        {
            pipe.ProcessOne();
            var msg = msgWriter.DequeueMessage();

            return msg;
        }

        #region IFudgeDeserializer Members

        /// <inheritdoc/>
        public FudgeContext Context
        {
            get { return context; }
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
                int refId = msgToIndexMap[subMsg];
                return (T)ProcessObject(refId, typeof(T), subMsg, out typeId);
            }
            else if (field.Type == IndicatorFieldType.Instance)
            {
                // Indicator means null
                return null;
            }
            else
            {
                int relativeRef = Convert.ToInt32(field.Value);
                int refIndex = relativeRef + stack.Peek().RefId;

                return (T)GetFromRef(refIndex);
            }
        }

        /// <inheritdoc/>
        public void Register(IFudgeFieldContainer msg, object obj)
        {
            // TODO 20100306 t0rx -- check state matches
            State state = stack.Peek();

            int index = state.RefId;
            Debug.Assert(objectList.Count > index);

            if (objectList[index].Obj != null)
            {
                throw new SerializationException("Attempt to register same deserialized object twice for type " + state.TypeName + " refID=" + index);
            }

            objectList[index].Obj = obj;
        }

        #endregion

        private sealed class State
        {
            private readonly FudgeContext context;
            private readonly int refId;
            private readonly Queue<IFudgeField> fields;
            private readonly string typeName;

            public State(FudgeMsg msg, int refId, string typeName)
            {
                this.context = msg.Context;
                this.fields = new Queue<IFudgeField>(msg.GetAllFields());
                this.refId = refId;
                this.typeName = typeName;
            }

            public int RefId
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
        }

        private class MsgAndObj
        {
            public FudgeMsg Msg;
            public object Obj;
        }
    }
}
