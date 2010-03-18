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
using System.Runtime.Serialization;

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
        private readonly Func<FudgeContext, TypeData, IFudgeSerializationSurrogate>[] selectors;
        private ISurrogateSelector dotNetSurrogateSelector = null;
        
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
            this.selectors = BuildSelectorList();
        }

        /// <summary>
        /// Gets or sets an <see cref="ISurrogateSelector"/> which provides surrogates implementing
        /// <see cref="ISerializationSurrogate"/> to allow old code to use Fudge serialization.
        /// </summary>
        public ISurrogateSelector DotNetSurrogateSelector
        {
            get { return dotNetSurrogateSelector; }
            set { dotNetSurrogateSelector = value; }
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

            foreach (var selector in selectors)
            {
                IFudgeSerializationSurrogate surrogate = selector(context, typeData);
                if (surrogate != null)
                    return surrogate;
            }

            throw new FudgeRuntimeException("Cannot automatically determine surrogate for type " + type.FullName);
        }

        private Func<FudgeContext, TypeData, IFudgeSerializationSurrogate>[] BuildSelectorList()
        {
            // This is the list of potential surrogates, in the order that they are tested
            return new Func<FudgeContext, TypeData, IFudgeSerializationSurrogate>[]
            {
                this.SurrogateFromAttribute,
                (c, td) => SerializableSurrogate.CanHandle(td) ? new SerializableSurrogate(td.Type) : null,
                (c, td) => ArraySurrogate.CanHandle(td) ? new ArraySurrogate(c, td) : null,
                (c, td) => DictionarySurrogate.CanHandle(td) ? new DictionarySurrogate(c, td) : null,
                (c, td) => ListSurrogate.CanHandle(td) ? new ListSurrogate(c, td) : null,
                (c, td) => ToFromFudgeMsgSurrogate.CanHandle(td) ? new ToFromFudgeMsgSurrogate(c, td) : null,
                (c, td) => DotNetSerializableSurrogate.CanHandle(td) ? new DotNetSerializableSurrogate(c, td) : null,
                this.SurrogateFromDotNetSurrogateSelector,
                (c, td) => PropertyBasedSerializationSurrogate.CanHandle(td) ? new PropertyBasedSerializationSurrogate(c, td) : null,
                (c, td) => ImmutableSurrogate.CanHandle(td) ? new ImmutableSurrogate(c, td) : null,
            };
        }

        private IFudgeSerializationSurrogate SurrogateFromDotNetSurrogateSelector(FudgeContext context, TypeData typeData)
        {
            if (DotNetSurrogateSelector == null)
                return null;

            ISurrogateSelector selector;
            StreamingContext sc = new StreamingContext(StreamingContextStates.Persistence);
            ISerializationSurrogate dotNetSurrogate = DotNetSurrogateSelector.GetSurrogate(typeData.Type, sc, out selector);
            if (dotNetSurrogate == null)
                return null;

            return new DotNetSerializationSurrogateSurrogate(context, typeData, dotNetSurrogate, selector);
        }

        private IFudgeSerializationSurrogate SurrogateFromAttribute(FudgeContext context, TypeData typeData)
        {
            var surrogateAttribute = typeData.CustomAttributes.FirstOrDefault(attrib => attrib is FudgeSurrogateAttribute);
            if (surrogateAttribute != null)
            {
                return BuildSurrogate(typeData.Type, (FudgeSurrogateAttribute)surrogateAttribute);
            }
            return null;
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
