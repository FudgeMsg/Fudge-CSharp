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
            pipe.ProcessOne();
            var msg = msgWriter.DequeueMessage();

            WalkMessage(msg);

            object result = GetFromRef(0, null);

            return result;
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
                int refId = msgToIndexMap[subMsg];
                Debug.Assert(objectList[refId].Msg == subMsg);

                return (T)GetFromRef(refId, typeof(T));             // It is possible that we've already deserialized this, so we call GetFromRef rather than just processing the message
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

                return (T)GetFromRef(refIndex, typeof(T));
            }
        }

        /// <inheritdoc/>
        public void Register(IFudgeFieldContainer msg, object obj)
        {
            State state = stack.Peek();
            if (msg != state.Msg)
            {
                throw new InvalidOperationException("Registering object of type " + obj.GetType() + " for message that it did not originate from.");
            }

            int index = state.RefId;
            Debug.Assert(objectList.Count > index);

            if (objectList[index].Obj != null)
            {
                throw new SerializationException("Attempt to register same deserialized object twice for type " + obj.GetType() + " refID=" + index);
            }

            objectList[index].Obj = obj;
        }

        #endregion

        /// <summary>
        /// Finds all the sub-messages in advance so we know their indices and can deserialize out of order if needed
        /// </summary>
        private void WalkMessage(FudgeMsg msg)
        {
            // REVIEW 2010-03-06 t0rx -- This would be more efficient if done at the same time as streaming in rather than separately
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

        /// <summary>
        /// Get the real object from a reference ID
        /// </summary>
        /// <remarks>It is possible that the reference has not yet been deserialized if (for example) it is a child of
        /// an object that has evolved elsewhere but where in this version that field has not been read.</remarks>
        private object GetFromRef(int? refId, Type hintType)
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
                DeserializeFromMessage(index, hintType);

                Debug.Assert(msgAndObj.Obj != null);
            }
            return msgAndObj.Obj;
        }

        private object DeserializeFromMessage(int index, Type hintType)
        {
            Debug.Assert(objectList[index].Obj == null);
            Debug.Assert(objectList[index].Msg != null);

            var message = objectList[index].Msg;
            objectList[index].Msg = null;                           // Just making sure we don't try to process the same one twice

            Type objectType = GetObjectType(index, hintType, message);
            var surrogate = GetSurrogate(objectType);

            var state = new State(message, index);
            stack.Push(state);
            object result = surrogate.Deserialize(message, this);
            stack.Pop();

            // Make sure the object was registered by the surrogate
            if (objectList[index].Obj == null || objectList[index].Obj != result)
            {
                throw new SerializationException("Object not registered during deserialization with type " + result.GetType());
            }

            return result;
        }

        private IFudgeSerializationSurrogate GetSurrogate(Type objectType)
        {
            int typeId = typeMap.GetTypeId(objectType);
            var surrogate = typeMap.GetSurrogate(typeId);
            if (surrogate == null)
            {
                throw new SerializationException("Type ID " + typeId + " not registered with serialization type map");
            }
            return surrogate;
        }

        private Type GetObjectType(int refId, Type hintType, FudgeMsg message)
        {
            Type objectType = null;
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
                string typeName = (string)typeField.Value;
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
                // We've got a type field, but it's not a string so it must be a reference back to where we last saw the type
                int previousObjId = refId + Convert.ToInt32(typeField.Value);

                if (previousObjId < 0 || previousObjId >= refId)
                {
                    throw new FudgeRuntimeException("Illegal relative type ID in sub-message: " + typeField.Value);
                }

                if (objectList[previousObjId].Obj != null)
                {
                    // Already deserialized it
                    objectType = objectList[previousObjId].Obj.GetType();
                }
                else
                {
                    // Scan it's fields rather than deserializing (we don't have the same type hint as might be in its correct location)
                    objectType = GetObjectType(previousObjId, hintType, objectList[previousObjId].Msg);
                }
            }
            return objectType;
        }

        private sealed class State
        {
            private readonly FudgeMsg msg;
            private readonly int refId;

            public State(FudgeMsg msg, int refId)
            {
                this.msg = msg;
                this.refId = refId;
            }

            public FudgeMsg Msg
            {
                get { return msg; }
            }

            public int RefId
            {
                get { return refId; }
            }
        }

        private class MsgAndObj
        {
            public FudgeMsg Msg;
            public object Obj;
        }
    }
}
