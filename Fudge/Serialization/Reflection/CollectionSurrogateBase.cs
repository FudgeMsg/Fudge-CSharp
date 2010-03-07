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
        /// <summary>The <see cref="FudgeContext"/> used by the surrogate.</summary>
        protected readonly FudgeContext context;
        /// <summary>The <see cref="TypeData"/> for the type this surrogate serializes.</summary>
        protected readonly TypeData typeData;
        /// <summary>The delegate to perform the serialization of this type.</summary>
        protected readonly Action<object, IAppendingFudgeFieldContainer, IFudgeSerializer> serializerDelegate;
        /// <summary>The delegate to perform the deserialization of this type.</summary>
        protected readonly Func<IFudgeFieldContainer, IFudgeDeserializer, object> deserializerDelegate;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="typeData"></param>
        /// <param name="serializeMethodName">Name of method to use to serialize objects.</param>
        /// <param name="deserializeMethodName">Name of method to use to deserialize objects.</param>
        /// <remarks>
        /// The <see cref="CollectionSurrogateBase"/> will scan for generic methods with the right names, and specialise them for
        /// the types given within <see cref="typeData"/>.
        /// </remarks>
        public CollectionSurrogateBase(FudgeContext context, TypeData typeData, string serializeMethodName, string deserializeMethodName)
        {
            this.context = context;
            this.typeData = typeData;
            Type[] types = (typeData.SubType2 == null) ? new Type[] {typeData.SubType} : new Type[] {typeData.SubType, typeData.SubType2};
            serializerDelegate = CreateMethodDelegate<Action<object, IAppendingFudgeFieldContainer, IFudgeSerializer>>(serializeMethodName, types);
            deserializerDelegate = CreateMethodDelegate<Func<IFudgeFieldContainer, IFudgeDeserializer, object>>(deserializeMethodName, types);
        }

        /// <summary>
        /// Creates a delegate for a method after specializing its generic types.
        /// </summary>
        /// <typeparam name="T">Type of delegate to create.</typeparam>
        /// <param name="name">Name of method to find.</param>
        /// <param name="genericTypes">Array of types to apply to the generic parameters of the method.</param>
        /// <returns>Delegate that calls the specialized method.</returns>
        protected T CreateMethodDelegate<T>(string name, Type[] genericTypes) where T : class
        {
            var method = this.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(genericTypes);
            return Delegate.CreateDelegate(typeof(T), this, method) as T;
        }

        #region IFudgeSerializationSurrogate Members

        /// <inheritdoc/>
        public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            serializerDelegate(obj, msg, serializer);
        }

        /// <inheritdoc/>
        public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return deserializerDelegate(msg, deserializer);
        }

        #endregion

        /// <summary>
        /// Helper method to serialize list contents.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="msg"></param>
        /// <param name="serializer"></param>
        protected void SerializeList<T>(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)    
        {
            var list = (IList<T>)obj;
            SerializeList(list, msg, serializer, typeData.SubTypeData.Kind, null);
        }

        /// <summary>
        /// Helper method to serialize list contents.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="msg"></param>
        /// <param name="serializer"></param>
        /// <param name="kind"></param>
        /// <param name="ordinal"></param>
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

        /// <summary>
        /// Helper method to deserialize an individual field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="deserializer"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
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
