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
    /// <summary>
    /// Base class for surrogates handling standard collection types.
    /// </summary>
    public abstract class CollectionSurrogateBase : IFudgeSerializationSurrogate
    {
        protected readonly FudgeContext context;
        protected readonly TypeData typeData;
        protected readonly Action<object, IAppendingFudgeFieldContainer, IFudgeSerializer> serializerDelegate;
        protected readonly Func<IFudgeFieldContainer, IFudgeDeserializer, object> deserializerDelegate;

        public CollectionSurrogateBase(FudgeContext context, TypeData typeData, string serializeMethodName, string deserializeMethodName)
        {
            this.context = context;
            this.typeData = typeData;
            Type[] types = (typeData.SubType2 == null) ? new Type[] {typeData.SubType} : new Type[] {typeData.SubType, typeData.SubType2};
            serializerDelegate = CreateMethodDelegate<Action<object, IAppendingFudgeFieldContainer, IFudgeSerializer>>(serializeMethodName, types);
            deserializerDelegate = CreateMethodDelegate<Func<IFudgeFieldContainer, IFudgeDeserializer, object>>(deserializeMethodName, types);
        }

        protected T CreateMethodDelegate<T>(string name, Type[] genericTypes) where T : class
        {
            var method = this.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(genericTypes);
            return Delegate.CreateDelegate(typeof(T), this, method) as T;
        }

        #region IFudgeSerializationSurrogate Members

        public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            serializerDelegate(obj, msg, serializer);
        }

        public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return deserializerDelegate(msg, deserializer);
        }

        #endregion

        protected void SerializeList<T>(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)    
        {
            var list = (IList<T>)obj;
            SerializeList(list, msg, serializer, typeData.SubTypeData.Kind, null);
        }

        protected static void SerializeList<T>(IEnumerable<T> list, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer, TypeData.TypeKind kind, int? ordinal)
        {
            switch (kind)
            {
                case TypeData.TypeKind.FudgePrimitive:
                case TypeData.TypeKind.Reference:
                    msg.AddAll(null, ordinal, list);
                    break;
                case TypeData.TypeKind.Inline:
                    serializer.WriteAllInline(msg, null, ordinal, list);
                    break;
            }
        }

        protected T DeserializeField<T>(IFudgeField field, IFudgeDeserializer deserializer, TypeData.TypeKind kind) where T : class
        {
            switch (kind)
            {
                case TypeData.TypeKind.FudgePrimitive:
                    return (T)context.TypeHandler.ConvertType(field.Value, typeof(T));
                case TypeData.TypeKind.Inline:
                case TypeData.TypeKind.Reference:
                    return deserializer.FromField<T>(field);
                default:
                    throw new FudgeRuntimeException("Unknown TypeData.TypeKind: " + kind);
            }
        }
    }
}
