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
    /// Implement <c>IFudgeSerializable</c> to allow your class to serialize and deserialize to
    /// Fudge message streams.
    /// </summary>
    public interface IFudgeSerializable
    {
        /// <summary>
        /// Serializes the object to a Fudge serializer.
        /// </summary>
        /// <param name="serializer">Serializer to receive the data.</param>
        void Serialize(IFudgeSerializer serializer);

        /// <summary>
        /// Begins the deserialization process the object.
        /// </summary>
        /// <param name="deserializer">Deserializer providing the data.</param>
        /// <param name="dataVersion">Version of the message data structure.</param>
        void BeginDeserialize(IFudgeDeserializer deserializer, int dataVersion);

        /// <summary>
        /// Deserializes the contents of the field into the object.
        /// </summary>
        /// <param name="deserializer">Deserializer providing the data.</param>
        /// <param name="field">Data to deserialize.</param>
        /// <param name="dataVersion">Version of the message data structure.</param>
        /// <returns><c>true</c> if the field was consumed, or <c>false</c> if the field is unused.</returns>
        /// <remarks>Unused fields can be collected in <see cref="EndDeserialize"/> to support evolvability of data.</remarks>
        bool DeserializeField(IFudgeDeserializer deserializer, IFudgeField field, int dataVersion);

        /// <summary>
        /// Called after all data for an object have been processed, to enable tidy-up.
        /// </summary>
        /// <param name="deserializer">Deserializer providing the data.</param>
        /// <param name="dataVersion">Version of the message data structure.</param>
        /// <remarks>Evolvable objects should call <see cref="IFudgeDeserializer.GetUnreadFields"/> here to obtain
        /// any fields that were not directly consumed by the object in <see cref="DeserializeField"/>.</remarks>
        void EndDeserialize(IFudgeDeserializer deserializer, int dataVersion);
    }
}
