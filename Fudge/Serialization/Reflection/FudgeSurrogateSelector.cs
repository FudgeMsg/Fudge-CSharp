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
using System.Diagnostics;

namespace Fudge.Serialization.Reflection
{
    /// <summary>
    /// Internal class to help choose a surrogate that is used to serialize a given type.
    /// </summary>
    /// <remarks>
    /// The <see cref="FudgeSurrogateSelector"/> will automatically handle lists, dictionaries and arrays, and
    /// where possible create surrogates for other types (for example if they have properties with getters and
    /// setters, or implement <see cref="IFudgeSerializable"/>).  It also follows the
    /// <see cref="FudgeSurrogateAttribute"/> attribute to specify another class that is the surrogate for the
    /// given type.
    /// </remarks>
    public class FudgeSurrogateSelector
    {
        private readonly FudgeContext context;
        private readonly TypeDataCache typeDataCache;
        
        /// <summary>
        /// Constructs a new <see cref="FudgeSurrogateSelector"/>.
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> for this selector.</param>
        public FudgeSurrogateSelector(FudgeContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this.context = context;
            this.typeDataCache = new TypeDataCache(context);
        }

        /// <summary>
        /// Creates a surrogate for a given type.
        /// </summary>
        /// <param name="type">Type for which to get surrogate.</param>
        /// <param name="fieldNameConvention">Convention for mapping .net property names to serialized field names.</param>
        /// <returns>Surrogate for the type.</returns>
        /// <exception cref="FudgeRuntimeException">Thrown if no surrogate can be automatically created.</exception>
        public IFudgeSerializationSurrogate GetSurrogate(Type type, FudgeFieldNameConvention fieldNameConvention)
        {
            var typeData = typeDataCache.GetTypeData(type, fieldNameConvention);

            // Look for FudgeSurrogate attribute
            var surrogateAttribute = typeData.CustomAttributes.FirstOrDefault(attrib => attrib is FudgeSurrogateAttribute);
            if (surrogateAttribute != null)
            {
                return BuildSurrogate(type, (FudgeSurrogateAttribute)surrogateAttribute);
            }

            // For all of these known types, we only need one surrogate as it is stateless
            IFudgeSerializationSurrogate surrogate;
            if (typeof(IFudgeSerializable).IsAssignableFrom(type))
            {
                surrogate = new SerializableSurrogate(type);
            }
            else if (ArraySurrogate.CanHandle(typeData))
            {
                surrogate = new ArraySurrogate(context, typeData);
            }
            else if (DictionarySurrogate.CanHandle(typeData))
            {
                surrogate = new DictionarySurrogate(context, typeData);
            }
            else if (ListSurrogate.CanHandle(typeData))
            {
                surrogate = new ListSurrogate(context, typeData);
            }
            else if (ToFromFudgeMsgSurrogate.CanHandle(typeData))
            {
                surrogate = new ToFromFudgeMsgSurrogate(context, typeData);
            }
            else if (PropertyBasedSerializationSurrogate.CanHandle(typeData))
            {
                surrogate = new PropertyBasedSerializationSurrogate(context, typeData);
            }
            else
            {
                throw new FudgeRuntimeException("Cannot automatically determine surrogate for type " + type.FullName);
            }
            return surrogate;
        }

        private IFudgeSerializationSurrogate BuildSurrogate(Type type, FudgeSurrogateAttribute attrib)
        {
            var surrogateType = attrib.SurrogateType;
            var constructor = surrogateType.GetConstructor(new Type[] { typeof(FudgeContext), typeof(Type) });
            object[] args;
            if (constructor != null)
            {
                args = new object[] { context, type };
            }
            else if ((constructor = surrogateType.GetConstructor(new Type[] { typeof(Type) })) != null)
            {
                args = new object[] { type };
            }
            else if ((constructor = surrogateType.GetConstructor(Type.EmptyTypes)) != null)
            {
                args = new object[] { };
            }
            else
            {
                Debug.Assert(false, "Lack of suitable constructor should have been picked up by FudgeSurrogateAttribute");
                throw new FudgeRuntimeException("Surrogate type " + surrogateType + " does not have appropriate constructor");
            }

            return (IFudgeSerializationSurrogate)constructor.Invoke(args);
        }
    }
}
