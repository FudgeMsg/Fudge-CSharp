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

namespace Fudge.Serialization.Reflection
{
    /// <summary>
    /// Handles serialization and deserialization of generic dictionaries.
    /// </summary>
    public class DictionarySurrogate : CollectionSurrogateBase
    {
        private const int keysOrdinal = 1;
        private const int valuesOrdinal = 2;

        /// <summary>
        /// Constructs a new instance for a specific dictionary type
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> for this surrogate.</param>
        /// <param name="typeData"><see cref="TypeData"/> describing the type to serialize.</param>
        public DictionarySurrogate(FudgeContext context, TypeData typeData)
            : base(context, typeData, "SerializeDictionary", "DeserializeDictionary")
        {
        }

        /// <summary>
        /// Detects whether a given type can be serialized with this class.
        /// </summary>
        /// <param name="typeData">Type to test.</param>
        /// <returns><c>true</c> if this class can handle the type.</returns>
        public static bool CanHandle(TypeData typeData)
        {
            return IsDictionary(typeData.Type);
        }

        /// <summary>
        /// Detects whether a given type is a generic dictionary.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns><c>true</c> if the type is a dictionary.</returns>
        public static bool IsDictionary(Type type)
        {
            Type keyType, valueType;
            return IsDictionary(type, out keyType, out valueType);
        }

        /// <summary>
        /// Detects whether a given type is a generic dictionary and obtains the key and value types.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <param name="keyType">Returns the type of the keys.</param>
        /// <param name="valueType">Returns the type of the values.</param>
        /// <returns><c>true</c> if the type is a dictionary.</returns>
        public static bool IsDictionary(Type type, out Type keyType, out Type valueType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                // It's a dictionary
                keyType = type.GetGenericArguments()[0];
                valueType = type.GetGenericArguments()[1];
                return true;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    // It's a dictionary
                    keyType = type.GetGenericArguments()[0];
                    valueType = type.GetGenericArguments()[1];
                    return true;
                }
            }

            keyType = null;
            valueType = null;
            return false;
        }

        private void SerializeDictionary<K, V>(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            var dictionary = (IDictionary<K, V>)obj;

            SerializeList(dictionary.Keys, msg, serializer, typeData.SubTypeData.Kind, keysOrdinal);
            SerializeList(dictionary.Values, msg, serializer, typeData.SubType2Data.Kind, valuesOrdinal);    // Guaranteed to be matching order
        }

        private object DeserializeDictionary<K, V>(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
            where K : class
            where V : class
        {
            var result = new Dictionary<K, V>(msg.GetNumFields());
            deserializer.Register(msg, result);

            var keys = new List<K>();
            var values = new List<V>();

            foreach (var field in msg)
            {
                if (field.Ordinal == 1)
                {
                    keys.Add(DeserializeField<K>(field, deserializer, typeData.SubTypeData.Kind));
                }
                else if (field.Ordinal == 2)
                {
                    values.Add(DeserializeField<V>(field, deserializer, typeData.SubType2Data.Kind));
                }
                else
                {
                    throw new FudgeRuntimeException("Sub-message doesn't contain a map (bad field " + field + ")");
                }
            }

            int nVals = Math.Min(keys.Count, values.Count);         // Consistent with Java implementation, rather than throwing an exception if they don't match
            for (int i = 0; i < nVals; i++)
            {
                result[keys[i]] = values[i];
            }

            return result;
        }
    }
}
