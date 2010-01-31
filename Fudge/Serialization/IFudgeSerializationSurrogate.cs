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
        /// <param name="serializer">Serializer to receive the data.</param>
        void Serialize(object obj, IFudgeSerializer serializer);

        /// <summary>
        /// Begins the deserialization process for a new object.
        /// </summary>
        /// <param name="deserializer">Deserializer providing the data.</param>
        /// <param name="dataVersion">Version of the message data structure.</param>
        /// <returns>State object to be used in <see cref="DeserializeField"/> and <see cref="EndDeserialize"/>.</returns>
        /// <remarks>
        /// <para>Surrogates typically follow two patterns - they either process each field as it is streamed in through
        /// <see cref="DeserializeField"/>, or they call <see cref="IFudgeDeserializer.GetUnreadFields"/> in <c>BeginDeserialize</c>
        /// to get all the fields as a <see cref="IFudgeFieldContainer"/> and then process directly.</para>
        /// <para>The state returned from <c>BeginDeserialize</c> is useful in situations where a single surrogate instance
        /// is used to deserialize multiple real objects to avoid constructing a new one each time.</para>
        /// </remarks>
        object BeginDeserialize(IFudgeDeserializer deserializer, int dataVersion);

        /// <summary>
        /// Deserializes the contents of the field into the object.
        /// </summary>
        /// <param name="deserializer">Deserializer providing the data.</param>
        /// <param name="field">Data to deserialize.</param>
        /// <param name="dataVersion">Version of the message data structure.</param>
        /// <param name="state">State created in <see cref="BeginDeserialize"/>.</param>
        /// <returns><c>true</c> if the field was consumed, or <c>false</c> if the field is unused.</returns>
        /// <remarks>Unused fields can be collected in <see cref="EndDeserialize"/> to support evolvability of data.</remarks>
        bool DeserializeField(IFudgeDeserializer deserializer, IFudgeField field, int dataVersion, object state);

        /// <summary>
        /// Called after all data for an object have been processed, to enable tidy-up.
        /// </summary>
        /// <param name="deserializer">Deserializer providing the data.</param>
        /// <param name="dataVersion">Version of the message data structure.</param>
        /// <param name="state">State created in <see cref="BeginDeserialize"/>.</param>
        /// <returns>Newly deserialized object.</returns>
        /// <remarks>Surrogates for evolvable objects should call <see cref="IFudgeDeserializer.GetUnreadFields"/> here to obtain
        /// any fields that were not directly consumed by the object in <see cref="DeserializeField"/>.</remarks>
        object EndDeserialize(IFudgeDeserializer deserializer, int dataVersion, object state);
    }
}
