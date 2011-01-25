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
using System.Reflection;
using System.Diagnostics;

namespace Fudge.Serialization.Reflection
{
    /// <summary>
    /// Handles serialization and deserialization of classes that have been written with
    /// the WCF <c>[DataContract]</c> marker.
    /// </summary>
    public class DataContractSurrogate : IFudgeSerializationSurrogate
    {
        private readonly FudgeContext context;
        private readonly Type type;
        private readonly PropertyBasedSerializationSurrogate.PropertySerializerMixin helper;

        /// <summary>
        /// Constructs a new instance for a specific type
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> for this surrogate.</param>
        /// <param name="typeData"><see cref="TypeData"/> describing the type to serialize.</param>
        public DataContractSurrogate(FudgeContext context, TypeData typeData)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (typeData == null)
                throw new ArgumentNullException("typeData");
            if (!CanHandle(typeData))
                throw new ArgumentOutOfRangeException("typeData", "ImmutableSurrogate cannot handle " + typeData.Type.FullName);

            this.context = context;
            this.type = typeData.Type;

            Debug.Assert(typeData.DefaultConstructor != null);      // Should have been caught in CanHandle()

            var properties = from prop in typeData.Properties.Concat(typeData.Fields)
                             where prop.GetCustomAttribute<DataMemberAttribute>() != null
                             select prop;

            this.helper = new PropertyBasedSerializationSurrogate.PropertySerializerMixin(context, typeData, properties, new DotNetSerializableSurrogate.BeforeAfterMethodMixin(context, typeData));
        }

        /// <summary>
        /// Determines whether a given type can be serialized with this class.
        /// </summary>
        /// <param
        /// name="typeData">Type to test.</param>
        /// <returns><c>true</c> if this class can handle the type.</returns>
        public static bool CanHandle(TypeData typeData)
        {
            return (typeData.GetCustomAttribute<DataContractAttribute>() != null);
        }

        #region IFudgeSerializationSurrogate Members

        /// <inheritdoc/>
        public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            helper.Serialize(obj, msg, serializer);
        }

        /// <inheritdoc/>
        public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return helper.CreateAndDeserialize(msg, deserializer);
        }

        #endregion
    }
}
