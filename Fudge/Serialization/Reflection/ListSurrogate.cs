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
    /// Handles serialization and deserialization of generic lists.
    /// </summary>
    public class ListSurrogate : CollectionSurrogateBase
    {
        /// <summary>
        /// Constructs a new instance for a specific list type
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> for this surrogate.</param>
        /// <param name="typeData"><see cref="TypeData"/> describing the type to serialize.</param>
        public ListSurrogate(FudgeContext context, TypeData typeData)
            : base(context, typeData, "SerializeList", "DeserializeList")
        {
        }

        /// <summary>
        /// Detects whether a given type can be serialized with this class.
        /// </summary>
        /// <param name="typeData">Type to test.</param>
        /// <returns><c>true</c> if this class can handle the type.</returns>
        public static bool CanHandle(TypeData typeData)
        {
            Type elementType;
            return IsList(typeData.Type, out elementType);
        }

        /// <summary>
        /// Detects whether a given type is a generic list.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns><c>true</c> if the type is a list.</returns>
        public static bool IsList(Type type)
        {
            Type elementType;
            return IsList(type, out elementType);
        }

        /// <summary>
        /// Detects whether a given type is a generic dictionary and obtains the element type.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <param name="elementType">Returns the type of the elements.</param>
        /// <returns><c>true</c> if the type is a list.</returns>
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

        private object DeserializeList<T>(IFudgeFieldContainer msg, IFudgeDeserializer deserializer) where T : class
        {
            var result = new List<T>(msg.GetNumFields());
            deserializer.Register(msg, result);
            foreach (var field in msg)
            {
                result.Add(DeserializeField<T>(field, deserializer, typeData.SubTypeData.Kind));
            }
            return result;
        }
    }
}
