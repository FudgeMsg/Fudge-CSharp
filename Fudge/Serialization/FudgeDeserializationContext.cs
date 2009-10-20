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
using System.Runtime.Serialization;
using OpenGamma.Fudge.Types;

namespace OpenGamma.Fudge.Serialization
{
    public class FudgeDeserializationContext : IFudgeDeserializationContext
    {
        private readonly SerializationMessage serMessage;
        private readonly List<object> reffedObjects;        // Holds objects that have been (or are being deserialised) that are references
        private readonly SerializationTypeMap typeMap;
        private readonly Stack<int> workingStack;

        public FudgeDeserializationContext(FudgeSerializer serializer, SerializationMessage message)
        {
            this.serMessage = message;
            this.reffedObjects = new List<object>();
            this.typeMap = serializer.TypeMap;
            this.workingStack = new Stack<int>();
        }

        #region IFudgeDeserializationContext Members

        public T FromMsg<T>(FudgeMsg msg) where T : class
        {
            int typeId = typeMap.GetTypeId(typeof(T));
            var surrogate = typeMap.GetSurrogate(typeId);
            if (surrogate == null)
            {
                throw new SerializationException("Cannot find surrogate to deserialize type " + typeof(T).FullName);
            }
            int typeVersion = typeMap.GetTypeVersion(typeId);
            try
            {
                workingStack.Push(-1);      // Just in case they try to register
                return (T)surrogate.Deserialize(msg, typeVersion, this);
            }
            finally
            {
                workingStack.Pop();
            }
        }

        public T FromRef<T>(int? refId) where T : class
        {
            if (refId == null)
            {
                return null;
            }

            if (refId < 0)
            {
                throw new SerializationException("Attempt to access negative referece ID");
            }
            if (refId < reffedObjects.Count)
            {
                // Already got
                return (T)reffedObjects[refId.Value];
            }

            // Need to deserialize
            if (refId >= serMessage.EncodedObjectCount)
            {
                throw new SerializationException(string.Format("Attempt to access reference ({0}) beyond end ({1})", refId, serMessage.EncodedObjectCount - 1));
            }
            var msg = serMessage.GetEncodedObject(refId.Value);

            try
            {
                workingStack.Push(refId.Value);       // Keep track of what we're up to to help with circular references

                return (T)DeserializeReference(msg, refId.Value, typeof(T));
            }
            finally
            {
                workingStack.Pop();
            }
        }

        public T FromField<T>(IFudgeField field) where T : class
        {
            if (field == null)
            {
                return null;
            }

            if (field.Type == FudgeMsgFieldType.Instance)
            {
                return FromMsg<T>((FudgeMsg)field.Value);
            }
            else
            {
                return FromRef<T>(Convert.ToInt32(field.Value));
            }
        }

        public void Register(object obj)
        {
            int refId = workingStack.Peek();
            if (refId == -1)
                return;         // Called when not deserialising a reference

            int currentCount = reffedObjects.Count;
            if (currentCount > refId)
            {
                // Already got it
                if (reffedObjects[refId] != obj)
                {
                    throw new SerializationException("Attempt to register a different object from that already registered for reference ID " + refId);
                }
            }
            else if (refId == currentCount)
            {
                // Just tag on the end
                reffedObjects.Add(obj);
            }
            else
            {
                // Need to add some padding
                object[] newEntries = new object[refId - currentCount + 1];
                newEntries[refId - currentCount] = obj;
                reffedObjects.AddRange(newEntries);
            }
        }

        #endregion

        private object DeserializeReference(FudgeMsg msg, int refId, Type hintType)
        {
            int? typeId = msg.GetInt(FudgeSerializer.TypeIdFieldName);
            if (typeId == null)
            {
                throw new SerializationException("No typeId found for object with reference ID " + refId);
            }

            var surrogate = typeMap.GetSurrogate(typeId.Value);
            if (surrogate == null)
            {
                throw new SerializationException("Type not registered for type ID " + typeId);
            }

            int typeVersion = typeMap.GetTypeVersion(typeId.Value);
            var result = surrogate.Deserialize(msg, typeVersion, this);
            Register(result);
            return result;
        }
    }
}
