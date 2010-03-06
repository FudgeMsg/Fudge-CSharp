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
using System.Diagnostics;
using System.Reflection;

namespace Fudge.Serialization
{
    /// <summary>
    /// <c>SerializableSurrogate</c> acts as a surrogate for objects that implement <see cref="IFudgeSerializable"/>.
    /// </summary>
    /// <remarks>
    /// You should not normally need to use this class directly.
    /// </remarks>
    public class SerializableSurrogate : IFudgeSerializationSurrogate
    {
        private readonly Type type;
        private readonly ConstructorInfo constructor;

        /// <summary>
        /// Constructs a new <c>SerializableSurrogate</c> for a given type.
        /// </summary>
        /// <param name="type">Type of the object, which must implement <see cref="IFudgeSerializable"/> and have a default constructor.</param>
        public SerializableSurrogate(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");            
            if (!typeof(IFudgeSerializable).IsAssignableFrom(type))
                throw new ArgumentOutOfRangeException("type");

            this.type = type;
            this.constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (constructor == null)
            {
                throw new FudgeRuntimeException("Type " + type.FullName + " does not have a public default constructor.");
            }
        }

        #region IFudgeSerializationSurrogate Members

        /// <inheritdoc/>
        public void Serialize(object obj, IMutableFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            IFudgeSerializable ser = (IFudgeSerializable)obj;
            ser.Serialize(msg, serializer);
        }

        /// <inheritdoc/>
        public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            IFudgeSerializable result = (IFudgeSerializable)constructor.Invoke(null);
            deserializer.Register(msg, result);
            result.Deserialize(msg, deserializer);
            return result;
        }

        #endregion
    }
}
