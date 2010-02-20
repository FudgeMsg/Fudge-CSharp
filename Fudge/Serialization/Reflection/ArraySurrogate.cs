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
    /// Handles serialization and deserialization of arrays.
    /// </summary>
    public class ArraySurrogate : CollectionSurrogateBase
    {
        /// <summary>
        /// Constructs a new instance for a specific array type
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> for this surrogate.</param>
        /// <param name="typeData"><see cref="TypeData"/> describing the type to serialize.</param>
        public ArraySurrogate(FudgeContext context, TypeData typeData)
            : base(context, typeData, "SerializeList", "DeserializeArray")
        {
        }

        /// <summary>
        /// Detects whether a given type can be serialized with this class.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns><c>true</c> if this class can handle the type.</returns>
        public static bool CanHandle(TypeData type)
        {
            return type.Type.IsArray;
        }

        private object DeserializeArray<T>(IFudgeFieldContainer msg, IFudgeDeserializer deserializer) where T : class
        {
            var list = (IList<T>)DeserializeList<T>(msg, deserializer);
            return list.ToArray();
        }
    }
}
