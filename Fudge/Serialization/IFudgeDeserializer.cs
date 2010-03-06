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
    /// <c>IFudgeDeserializer</c> is the interface through which objects being deserialized access the serialization framework.
    /// </summary>
    public interface IFudgeDeserializer
    {
        /// <summary>
        /// Gets the <see cref="FudgeContext"/> for this deserializer.
        /// </summary>
        FudgeContext Context { get; }

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
        /// <param name="msg">Message from which the object was deserialized.</param>
        /// <param name="obj">Object to register with the context.</param>
        /// <remarks>
        /// <para>
        /// Every new object must be registered after construction, and this process is usually
        /// performed by the surrogate (implementing <see cref="IFudgeSerializationSurrogate"/>)
        /// which performs the deserialization.  It is essential to register the new object
        /// before trying to deserialize any sub-objects which may potentially contain references
        /// back to this one (i.e. where there are cycles in the object graph).  The surrogate
        /// will usually therefore register the object immediately after construction and before any
        /// fields are deserialized.
        /// </para>
        /// <para>
        /// Failing to register a new object will cause an exception to be thrown by the serialization
        /// framework.  However, when using automatic (reflection-based) surrogates (e.g. when a class
        /// has a default constructor and implements <see cref="IFudgeSerializable"/> then this is
        /// done automatically.
        /// </para>
        /// </remarks>
        void Register(IFudgeFieldContainer msg, object obj);
    }
}
