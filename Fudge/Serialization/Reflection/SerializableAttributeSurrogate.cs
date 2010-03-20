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
using System.Runtime.Serialization;

namespace Fudge.Serialization.Reflection
{
    /// <summary>
    /// Handles serialization and deserialization of classes that are marked with <see cref="SerializableAttribute"/>
    /// but do not implement <see cref="ISerializable"/>.
    /// </summary>
    public class SerializableAttributeSurrogate : IFudgeSerializationSurrogate
    {
        private readonly PropertyBasedSerializationSurrogate.PropertySerializerMixin serializerMixin;
        private readonly Type type;

        /// <summary>
        /// Constructs a new <see cref="SerializableAttributeSurrogate"/>.
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> to use.</param>
        /// <param name="typeData"><see cref="TypeData"/> for the type for this surrogate.</param>
        public SerializableAttributeSurrogate(FudgeContext context, TypeData typeData)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (typeData == null)
                throw new ArgumentNullException("typeData");
            if (!CanHandle(typeData))
                throw new ArgumentOutOfRangeException("typeData", "SerializableAttributeSurrogate cannot handle " + typeData.Type.FullName);

            this.type = typeData.Type;

            var fields = from field in typeData.Fields
                         where field.GetCustomAttribute<NonSerializedAttribute>() == null
                         select field;

            var beforeAfterMixin = new DotNetSerializableSurrogate.BeforeAfterMethodMixin(context, typeData);
            this.serializerMixin = new PropertyBasedSerializationSurrogate.PropertySerializerMixin(context, typeData, fields, beforeAfterMixin);
        }

        /// <summary>
        /// Detects whether a given type can be serialized with this class.
        /// </summary>
        /// <param
        /// name="typeData">Type to test.</param>
        /// <returns><c>true</c> if this class can handle the type.</returns>
        public static bool CanHandle(TypeData typeData)
        {
            return typeData.GetCustomAttribute<SerializableAttribute>() != null;
        }

        #region IFudgeSerializationSurrogate Members

        /// <inheritdoc/>
        public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            serializerMixin.Serialize(obj, msg, serializer);
        }

        /// <inheritdoc/>
        public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return serializerMixin.CreateAndDeserialize(msg, deserializer);
        }

        #endregion
    }
}
