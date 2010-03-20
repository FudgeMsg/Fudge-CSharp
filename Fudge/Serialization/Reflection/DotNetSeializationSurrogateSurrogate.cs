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
    /// Surrogate that uses an <see cref="ISerializationSurrogate"/> from the .net serialization framework
    /// to do the serialization and deserialization.
    /// </summary>
    public class DotNetSerializationSurrogateSurrogate : IFudgeSerializationSurrogate
    {
        private readonly DotNetSerializableSurrogate.SerializationMixin helper;
        private readonly ISerializationSurrogate surrogate;
        private readonly ISurrogateSelector selector;

        /// <summary>
        /// Constructs a new <see cref="DotNetSerializationSurrogateSurrogate"/>.
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> to use.</param>
        /// <param name="typeData"><see cref="TypeData"/> for the type for this surrogate.</param>
        /// <param name="surrogate">Surrogate that maps the object to or from a <see cref="SerializationInfo"/>.</param>
        /// <param name="selector">Selector that produced the surrogate.</param>
        public DotNetSerializationSurrogateSurrogate(FudgeContext context, TypeData typeData, ISerializationSurrogate surrogate, ISurrogateSelector selector)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (typeData == null)
                throw new ArgumentNullException("typeData");
            if (surrogate == null)
                throw new ArgumentNullException("surrogate");
            // Don't care if selector is null
            
            this.helper = new DotNetSerializableSurrogate.SerializationMixin(context, typeData.Type, new DotNetSerializableSurrogate.BeforeAfterMethodMixin(context, typeData));
            this.surrogate = surrogate;
            this.selector = selector;
        }

        /// <summary>
        /// Gets the .net <see cref="ISerializationSurrogate"/> that will perform the serialization and deserialization.
        /// </summary>
        public ISerializationSurrogate SerializationSurrogate
        {
            get { return surrogate; }
        }

        #region IFudgeSerializationSurrogate Members

        /// <inheritdoc/>
        public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            helper.Serialize(msg, obj, surrogate.GetObjectData);
        }

        /// <inheritdoc/>
        public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return helper.Deserialize(msg, deserializer, (obj, si, sc) => { surrogate.SetObjectData(obj, si, sc, selector);});
        }

        #endregion
    }
}
