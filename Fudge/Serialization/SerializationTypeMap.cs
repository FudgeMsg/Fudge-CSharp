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
using Fudge.Serialization.Reflection;
using System.Diagnostics;

namespace Fudge.Serialization
{
    /// <summary>
    /// Holds the mapping of types to surrogates used to serialize or deserialize them.
    /// </summary>
    public class SerializationTypeMap
    {
        private readonly FudgeContext context;
        private readonly List<TypeData> typeDataList = new List<TypeData>();
        private readonly Dictionary<Type, int> typeMap = new Dictionary<Type, int>();
        private readonly FudgeSurrogateSelector surrogateSelector;

        /// <summary>
        /// Constructs a new <see cref="SerializationTypeMap"/>
        /// </summary>
        /// <param name="context"></param>
        public SerializationTypeMap(FudgeContext context)
        {
            this.context = context;
            this.surrogateSelector = new FudgeSurrogateSelector(context);
            this.AllowTypeDiscovery = (bool)context.GetProperty(ContextProperties.AllowTypeDiscoveryProperty, true);
            this.FieldNameConvention = (FudgeFieldNameConvention)context.GetProperty(ContextProperties.FieldNameConventionProperty, FudgeFieldNameConvention.Identity);
        }

        /// <summary>
        /// Gets or sets whether Fudge will automatically try to register types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this is <c>false</c>, then all types must be registered with the type map before
        /// serialization or deserialization.
        /// </para>
        /// <para>
        /// By default, this is <c>true</c>, i.e. types can be added automatically.  You can set the
        /// <see cref="ContextProperties.AllowTypeDiscoveryProperty"/> property in the <see cref="FudgeContext"/> before
        /// constructing a <c>FudgeSerializer</c> to override this default, or set
        /// <see cref="AllowTypeDiscovery"/> directly.
        /// </para>
        /// </remarks>
        public bool AllowTypeDiscovery
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the convention to use when converting .net property names to Fudge field names, by default Identity.
        /// </summary>
        /// <remarks>
        /// On construction, the <see cref="SerializationTypeMap"/> will pick up any default specified
        /// using the <see cref="ContextProperties.FieldNameConventionProperty"/> property in the <see cref="FudgeContext"/>
        /// of set <see cref="FudgeFieldNameConvention"/> directly.
        /// </remarks>
        /// <seealso cref="FudgeFieldNameConvention"/>
        public FudgeFieldNameConvention FieldNameConvention
        {
            get;
            set;
        }

        /// <summary>
        /// Registers a type, automatically generating a serialization surrogate.
        /// </summary>
        /// <param name="type">Type to register.</param>
        /// <returns></returns>
        public int RegisterType(Type type)
        {
            var surrogate = surrogateSelector.GetSurrogate(type, FieldNameConvention);
            Debug.Assert(surrogate != null);
            return RegisterType(type, surrogate);
        }

        /// <summary>
        /// Registers a type with a serialization surrogate.
        /// </summary>
        /// <param name="type">Type that the surrogate is for.</param>
        /// <param name="surrogate">Surrogate to serialize and deserialize the type.</param>
        public int RegisterType(Type type, IFudgeSerializationSurrogate surrogate)
        {
            int id = typeDataList.Count;
            var entry = new TypeData { Surrogate = surrogate, Type = type };
            typeDataList.Add(entry);
            typeMap.Add(type, id);
            return id;
        }

        /// <summary>
        /// Gets an ID for a type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>This is used by the serialization framework and would not normally be useful to developers.</remarks>
        public int GetTypeId(Type type)
        {
            int index;
            if (typeMap.TryGetValue(type, out index))
            {
                return index;
            }

            // Not found
            if (AllowTypeDiscovery)
            {
                return RegisterType(type);
            }

            return -1;
        }

        /// <summary>
        /// Returns the surrogate for a type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IFudgeSerializationSurrogate GetSurrogate(Type type)
        {
            int index;
            if (typeMap.TryGetValue(type, out index))
            {
                return typeDataList[index].Surrogate;
            }

            // Not found
            if (AllowTypeDiscovery)
            {
                int typeId = RegisterType(type);
                if (typeId != -1)
                    return typeDataList[typeId].Surrogate;
            }

            return null;
        }

        /// <summary>
        /// Gets the surrogate for a given type ID
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public IFudgeSerializationSurrogate GetSurrogate(int typeId)
        {
            if (typeId < 0 || typeId >= typeDataList.Count)
                return null;

            return typeDataList[typeId].Surrogate;
        }

        private class TypeData
        {
            public Type Type { get; set; }
            public IFudgeSerializationSurrogate Surrogate { get; set; }
        }
    }
}
