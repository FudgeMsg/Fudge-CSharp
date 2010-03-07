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
    public class SerializationTypeMap
    {
        private readonly FudgeContext context;
        private readonly List<TypeData> typeDataList = new List<TypeData>();
        private readonly Dictionary<Type, int> typeMap = new Dictionary<Type, int>();
        private readonly FudgeSurrogateSelector surrogateSelector;

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

        public int RegisterType(Type type)
        {
            var surrogateFactory = surrogateSelector.GetSurrogateFactory(type, FieldNameConvention);
            Debug.Assert(surrogateFactory != null);
            return RegisterType(type, surrogateFactory);
        }

        /// <summary>
        /// Registers a type with a serialization surrogate that needs no internal state.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="statelessSurrogate"></param>
        public void RegisterType(Type type, IFudgeSerializationSurrogate statelessSurrogate)
        {
            RegisterType(type, c => statelessSurrogate);
        }

        public int RegisterType(Type type, Func<FudgeContext, IFudgeSerializationSurrogate> surrogateFactory)
        {
            int id = typeDataList.Count;
            var entry = new TypeData { SurrogateFactory = surrogateFactory, Type = type };
            typeDataList.Add(entry);
            typeMap.Add(type, id);
            return id;
        }

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

        public Func<FudgeContext, IFudgeSerializationSurrogate> GetSurrogateFactory(Type type)
        {
            int index;
            if (typeMap.TryGetValue(type, out index))
            {
                return typeDataList[index].SurrogateFactory;
            }

            // Not found
            if (AllowTypeDiscovery)
            {
                int typeId = RegisterType(type);
                if (typeId != -1)
                    return typeDataList[typeId].SurrogateFactory;
            }

            return null;
        }

        public Func<FudgeContext, IFudgeSerializationSurrogate> GetSurrogateFactory(int typeId)
        {
            if (typeId < 0 || typeId >= typeDataList.Count)
                return null;

            return typeDataList[typeId].SurrogateFactory;
        }

        private class TypeData
        {
            public Type Type { get; set; }
            public Func<FudgeContext, IFudgeSerializationSurrogate> SurrogateFactory { get; set; }
        }
    }
}
