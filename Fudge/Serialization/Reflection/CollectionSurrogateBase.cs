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
using System.Reflection;

namespace Fudge.Serialization.Reflection
{
    public abstract class CollectionSurrogateBase : IFudgeSerializationSurrogate
    {
        protected readonly FudgeContext context;
        protected readonly TypeData typeData;
        protected readonly Action<object, IFudgeSerializer> serializerDelegate;
        protected readonly Func<IFudgeFieldContainer, IFudgeDeserializer, object> deserializerDelegate;

        public CollectionSurrogateBase(FudgeContext context, TypeData typeData, string serializeMethodName, string deserializeMethodName)
        {
            this.context = context;
            this.typeData = typeData;
            Type[] types = (typeData.SubType2 == null) ? new Type[] {typeData.SubType} : new Type[] {typeData.SubType, typeData.SubType2};
            serializerDelegate = CreateMethodDelegate<Action<object, IFudgeSerializer>>(serializeMethodName, types);
            deserializerDelegate = CreateMethodDelegate<Func<IFudgeFieldContainer, IFudgeDeserializer, object>>(deserializeMethodName, types);
        }


        protected T CreateMethodDelegate<T>(string name, Type valueType) where T : class
        {
            return CreateMethodDelegate<T>(name, new Type[] { valueType });
        }

        protected T CreateMethodDelegate<T>(string name, Type[] genericTypes) where T : class
        {
            var method = this.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(genericTypes);
            return Delegate.CreateDelegate(typeof(T), this, method) as T;
        }

        #region IFudgeSerializationSurrogate Members

        public void Serialize(object obj, IFudgeSerializer serializer)
        {
            serializerDelegate(obj, serializer);
        }

        public object BeginDeserialize(IFudgeDeserializer deserializer, int dataVersion)
        {
            var msg = deserializer.GetUnreadFields();
            return deserializerDelegate(msg, deserializer);
        }

        public bool DeserializeField(IFudgeDeserializer deserializer, IFudgeField field, int dataVersion, object state)
        {
            return false;
        }

        public object EndDeserialize(IFudgeDeserializer deserializer, int dataVersion, object state)
        {
            return state;
        }

        #endregion

        protected void SerializeList<T>(object obj, IFudgeSerializer serializer)    
        {
            var list = (IList<T>)obj;
            SerializeList(list, serializer, typeData.SubTypeData.Kind, null);
        }

        protected static void SerializeList<T>(IEnumerable<T> list, IFudgeSerializer serializer, TypeData.TypeKind kind, int? ordinal)
        {
            switch (kind)
            {
                case TypeData.TypeKind.FudgePrimitive:
                    serializer.WriteAll(null, ordinal, list);
                    break;
                case TypeData.TypeKind.Inline:
                    serializer.WriteAllSubMsgs(null, ordinal, list);
                    break;
                case TypeData.TypeKind.Object:
                    serializer.WriteAllRefs(null, ordinal, list);
                    break;
            }
        }

        protected object DeserializeList<T>(IFudgeFieldContainer msg, IFudgeDeserializer deserializer) where T : class
        {
            var result = new List<T>(msg.GetNumFields());
            foreach (var field in msg)
            {
                result.Add(DeserializeField<T>(field, deserializer, typeData.SubTypeData.Kind));
            }
            return result;
        }

        protected T DeserializeField<T>(IFudgeField field, IFudgeDeserializer deserializer, TypeData.TypeKind kind) where T : class
        {
            switch (kind)
            {
                case TypeData.TypeKind.FudgePrimitive:
                    return (T)context.TypeHandler.ConvertType(field.Value, typeof(T));
                case TypeData.TypeKind.Inline:
                case TypeData.TypeKind.Object:
                    return deserializer.FromField<T>(field);
                default:
                    throw new FudgeRuntimeException("Unknown TypeData.TypeKind: " + kind);
            }
        }

    }
}
