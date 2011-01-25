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

namespace Fudge.Serialization
{
    /// <summary>
    /// <c>IFudgeSerializationSurrogate</c> performs the serialization and deserialization on behalf of another class.
    /// </summary>
    /// <remarks>
    /// A surrogate is typically used in situations where the main class cannot be modified, where there is a desire to
    /// keep Fudge-related code separate from the main code base, or where a class is immutable (so all deserialized data
    /// must be collected before construction).  It is also necessary if an object may contain sub-objects that could
    /// create a circular reference, as the parent object must be registered with <see cref="IFudgeDeserializer.Register"/>
    /// post-construction but before the sub-objects are deserialized.
    /// </remarks>
    public interface IFudgeSerializationSurrogate
    {
        /// <summary>
        /// Serializes the given object to a Fudge serializer.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="msg">Message into which the data should be serialized.</param>
        /// <param name="serializer">Serializer controlling the serialization process.</param>
        void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer);

        /// <summary>
        /// Deserializes a message into a new object.
        /// </summary>
        /// <param name="msg">Message to deserialize from.</param>
        /// <param name="deserializer">Deserializer controlling the deserialization process.</param>
        /// <returns>Newly constructed and initialized object.</returns>
        /// <remarks>
        /// The surrogate must register the new object by calling <see cref="IFudgeDeserializer.Register"/>
        /// as soon after construction as possible to make the object available for any references
        /// back from contained objects.
        /// </remarks>
        object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer);
    }
}
