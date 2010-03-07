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
using System.Collections;
using System.Diagnostics;

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
        private readonly Dictionary<object, int> idMap;     // Tracks IDs of objects that have already been serialised (or are in the process)
        private readonly Dictionary<Type, int> lastTypes = new Dictionary<Type, int>();     // Tracks the last object of a given type
        private readonly SerializationTypeMap typeMap;
        private readonly IFudgeTypeMappingStrategy typeMappingStrategy;
        private readonly IndexedStack<State> inlineStack = new IndexedStack<State>();       // Used to check for cycles in inlined messages and keep track of the index of the current message
        private int currentMessageId = 0;

        public FudgeSerializationContext(FudgeContext context, SerializationTypeMap typeMap, IFudgeStreamWriter writer, IFudgeTypeMappingStrategy typeMappingStrategy)
        {
            this.context = context;
            this.writer = writer;
            this.idMap = new Dictionary<object, int>();     // TODO 2009-10-18 t0rx -- Worry about HashCode and Equals implementations
            this.typeMap = typeMap;
            this.typeMappingStrategy = typeMappingStrategy;
        }

        public void SerializeGraph(object graph)
        {
            Debug.Assert(currentMessageId == 0);

            RegisterObject(graph);

            var msg = new StreamingMessage(this);

            writer.StartMessage();
            WriteTypeInformation(graph, currentMessageId, msg);
            UpdateLastTypeInfo(graph);
            SerializeContents(graph, currentMessageId, msg);
            writer.EndMessage();
        }

        #region IFudgeSerializer Members

        /// <inheritdoc/>
        public FudgeContext Context
        {
            get { return context; }
        }

        /// <inheritdoc/>
        public void WriteInline(IAppendingFudgeFieldContainer msg, string fieldName, int? ordinal, object obj)
        {
            if (obj != null)
            {
                WriteObject(fieldName, ordinal, obj, false, false);
            }
        }

        #endregion

        private int RegisterObject(object obj)
        {
            idMap[obj] = currentMessageId;      // There is the possibility that an object is serialised in-line twice - we take the later so that relative references are smaller
            return currentMessageId;
        }

        private void SerializeContents(object obj, int index, IAppendingFudgeFieldContainer msg)
        {
            CheckForInlineCycles(obj);

            inlineStack.Push(new State(obj, index));

            var surrogateFactory = typeMap.GetSurrogateFactory(obj.GetType());
            if (surrogateFactory == null)
            {
                // Unknown type
                throw new ArgumentOutOfRangeException("Type \"" + obj.GetType().FullName + "\" not registered, cannot serialize");
            }
            var surrogate = surrogateFactory(context);

            surrogate.Serialize(obj, msg, this);

            inlineStack.Pop();
        }

        private void CheckForInlineCycles(object obj)
        {
            for (int i = inlineStack.Count - 1; i >= 0; i--)
            {
                if (inlineStack[i].Obj == obj)
                {
                    throw new FudgeRuntimeException("Cycle detected in inlined objects at object of type " + obj.GetType());
                }
            }
        }

        private void UpdateLastTypeInfo(object obj)
        {
            lastTypes[obj.GetType()] = currentMessageId;
        }

        private void WriteTypeInformation(object obj, int id, IAppendingFudgeFieldContainer msg)
        {
            Type type = obj.GetType();
            int lastSeen;
            if (lastTypes.TryGetValue(type, out lastSeen))
            {
                // Already had something of this type
                int offset = lastSeen - id;
                msg.Add(null, FudgeSerializer.TypeIdFieldOrdinal, PrimitiveFieldTypes.IntType, offset);
            }
            else
            {
                // Not seen before, so write out with base types
                for (Type currentType = type; currentType != typeof(object); currentType = currentType.BaseType)
                {
                    string typeName = typeMappingStrategy.GetName(currentType);
                    msg.Add(null, FudgeSerializer.TypeIdFieldOrdinal, StringFieldType.Instance, typeName);
                }
            }
        }

        // Write is called from the various StreamingMessage.Add methods - rather than it storing the value
        // we just write it out to the output stream
        private void Write(string fieldName, int? ordinal, FudgeFieldType type, object value)
        {            
            if (type == null)
            {
                type = context.TypeHandler.DetermineTypeFromValue(value);
            }
            if (type == null)
            {
                WriteObject(fieldName, ordinal, value, true, true);
            }
            else
            {
                if (type == FudgeMsgFieldType.Instance)
                {
                    // As references are based on the number of messages, we have to update to take this one into account
                    currentMessageId += CountMessages((IFudgeFieldContainer)value);
                }
                writer.WriteField(fieldName, ordinal, type, value);
            }
        }

        private int CountMessages(IFudgeFieldContainer value)
        {
            int count = 1;
            foreach (var field in value)
            {
                if (field.Type == FudgeMsgFieldType.Instance || (field.Type == null && field.Value is IFudgeFieldContainer))
                {
                    count += CountMessages((IFudgeFieldContainer)field.Value);
                }
            }
            return count;
        }

        private void WriteObject(string fieldName, int? ordinal, object value, bool allowRefs, bool writeTypeInfo)
        {
            if (allowRefs)
            {
                int previousId = GetRefId(value);
                if (previousId != -1)
                {
                    // Refs are relative to the containing message
                    int diff = previousId - inlineStack.Peek().Index;
                    writer.WriteField(fieldName, ordinal, PrimitiveFieldTypes.IntType, diff);
                    return;
                }
            }

            // New object
            currentMessageId++;
            RegisterObject(value);

            var subMsg = new StreamingMessage(this);
            writer.StartSubMessage(fieldName, ordinal);
            if (writeTypeInfo)
            {
                WriteTypeInformation(value, currentMessageId, subMsg);
            }
            UpdateLastTypeInfo(value);
            SerializeContents(value, currentMessageId, subMsg);
            writer.EndSubMessage();
        }

        private int GetRefId(object obj)
        {
            int id;
            if (!idMap.TryGetValue(obj, out id))
            {
                return -1;
            }

            return id;
        }

        /// <summary>
        /// StreamingMessage appears to the user like it is a normal message, but rather than adding
        /// fields it's actually streaming them out to the writer.
        /// </summary>
        private sealed class StreamingMessage : IAppendingFudgeFieldContainer
        {
            private readonly FudgeSerializationContext serializationContext;

            public StreamingMessage(FudgeSerializationContext serializationContext)
            {
                this.serializationContext = serializationContext;
            }

            #region IAppendingFudgeFieldContainer Members

            public void Add(IFudgeField field)
            {
                serializationContext.Write(field.Name, field.Ordinal, field.Type, field.Value);
            }

            public void Add(string name, object value)
            {
                serializationContext.Write(name, null, null, value);
            }

            public void Add(int? ordinal, object value)
            {
                serializationContext.Write(null, ordinal, null, value);
            }

            public void Add(string name, int? ordinal, object value)
            {
                serializationContext.Write(name, ordinal, null, value);
            }

            public void Add(string name, int? ordinal, FudgeFieldType type, object value)
            {
                serializationContext.Write(name, ordinal, type, value);
            }

            #endregion
        }

        /// <summary>
        /// You can't index into Stack{T} so this gives us something that we can index but looks like a stack
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private sealed class IndexedStack<T> : List<T>
        {
            public void Push(T val)
            {
                base.Add(val);
            }

            public T Pop()
            {
                int index = this.Count - 1;
                if (index == -1)
                    throw new InvalidOperationException();

                var result = this[index];
                this.RemoveAt(index);
                return result;
            }

            public T Peek()
            {
                int index = this.Count - 1;
                if (index == -1)
                    throw new InvalidOperationException();

                return this[index];
            }
        }

        private struct State
        {
            private readonly object obj;
            private readonly int index;

            public State(object obj, int index)
            {
                this.obj = obj;
                this.index = index;
            }

            public object Obj { get { return obj; } }
            public int Index { get { return index; } }
        }
    }
}
