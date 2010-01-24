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
    public interface IFudgeDeserializer
    {
        /// <summary>
        /// Gets any fields that have not been read by <see cref="IFudgeSerializable.DeserializeField"/> or <see cref="IFudgeSerializationSurrogate.DeserializeField"/>
        /// as an <see cref="IFudgeFieldContainer"/>.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This can be called in <c>BeginDeserialize</c> in which case all fields are returned, or <c>EndDeserialize</c> in which case all fields that were not
        /// handled in <c>DeserializeField</c> are returned.
        /// </remarks>
        IFudgeFieldContainer GetUnreadFields();

        /// <summary>
        /// Deserialises an object from a Fudge field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">Field containing either a reference to the object as an integer, or the object serialised as a <c>FudgeMsg</c></param>
        /// <returns>Deserialised object</returns>
        /// <remarks>
        /// This method allows the deserialiser to be agnostic to whether the object was serialised as a reference or in-place.
        /// </remarks>
        T FromField<T>(IFudgeField field) where T : class;

        /// <summary>
        /// Registers a partially-constructed object in case of reference cycles.
        /// </summary>
        /// <param name="obj">Object to register with the context.</param>
        /// <remarks>
        /// <para>
        /// If there is the possibility that your object may reference another object which
        /// may in turn directly or indirectly reference back to yours, then calling <c>Register</c>
        /// will allow this reference to be resolved.  This will typically be performed during
        /// <see cref="IFudgeSerializationSurrogate.Deserialize"/>  immediately after construction
        /// but before any members are initialised.
        /// </para>
        /// <para>
        /// If there is no possibility of cyclic references then is is not necessary to call this
        /// method.
        /// </para>
        /// </remarks>
        void Register(object obj);
    }
}
