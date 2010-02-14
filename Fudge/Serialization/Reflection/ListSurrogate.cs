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
    public class ListSurrogate : IFudgeSerializationSurrogate
    {
        private readonly FudgeContext context;
        private readonly TypeData typeData;
        private readonly Action<object, IFudgeSerializer> serializerDelegate;
        private readonly Func<IFudgeFieldContainer, IFudgeDeserializer, object> deserializerDelegate;

        public ListSurrogate(FudgeContext context, TypeData typeData)
        {
            this.context = context;
            this.typeData = typeData;
            serializerDelegate = CreateMethodDelegate<Action<object, IFudgeSerializer>>("SerializeList", typeData.SubType);
            deserializerDelegate = CreateMethodDelegate<Func<IFudgeFieldContainer, IFudgeDeserializer, object>>("DeserializeList", typeData.SubType);
        }

        public static bool CanHandle(TypeData typeData)
        {
            Type elementType;
            return IsList(typeData.Type, out elementType);
        }

        public static bool IsList(Type type)
        {
            Type elementType;
            return IsList(type, out elementType);
        }

        public static bool IsList(Type type, out Type elementType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                // It's a list
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    // It's a list
                    elementType = type.GetGenericArguments()[0];
                    return true;
                }
            }

            elementType = null;
            return false;
        }

        private T CreateMethodDelegate<T>(string name, Type valueType) where T : class
        {
            // TODO 2010-02-14 t0rx -- Extract into helper class
            var method = this.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(new Type[] { valueType });
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

        private void SerializeList<T>(object obj, IFudgeSerializer serializer)
        {
            var list = (IList<T>)obj;
            switch (typeData.SubTypeData.Kind)
            {
                case TypeData.TypeKind.FudgePrimitive:
                    serializer.WriteAll(null, null, list);
                    break;
                case TypeData.TypeKind.Inline:
                    serializer.WriteAllSubMsgs(null, null, list);
                    break;
                case TypeData.TypeKind.Object:
                    serializer.WriteAllRefs(null, null, list);
                    break;
            }
        }

        private object DeserializeList<T>(IFudgeFieldContainer msg, IFudgeDeserializer deserializer) where T : class
        {
            var result = new List<T>(msg.GetNumFields());
            switch (typeData.SubTypeData.Kind)
            {
                case TypeData.TypeKind.FudgePrimitive:
                    foreach (var field in msg)
                    {
                        result.Add((T)context.TypeHandler.ConvertType(field.Value, typeof(T)));
                    }
                    break;
                case TypeData.TypeKind.Inline:
                case TypeData.TypeKind.Object:
                    foreach (var field in msg)
                    {
                        result.Add(deserializer.FromField<T>(field));
                    }
                    break;
            }
            return result;
        }
    }
}
