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
        private readonly Dictionary<string, int> nameMap = new Dictionary<string, int>();
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
        /// <see cref="FudgeSerializer.AllowTypeDiscoveryProperty"/> property in the <see cref="FudgeContext"/> before
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
        /// using the <see cref="FudgeSerializer.FieldNameConventionProperty"/> property in the <see cref="FudgeContext"/>
        /// of set <see cref="FudgeFieldNameConvention"/> directly.
        /// </remarks>
        /// <seealso cref="FudgeFieldNameConvention"/>
        public FudgeFieldNameConvention FieldNameConvention
        {
            get;
            set;
        }

        public int AutoRegister(Type type)
        {
            string name = type.FullName;        // TODO 2010-02-02 t0rx -- Allow user to override name with either a strategy or an attribute
            var surrogateFactory = surrogateSelector.GetSurrogateFactory(type, FieldNameConvention);
            Debug.Assert(surrogateFactory != null);
            int dataVersion = 0;                // TODO 2010-02-02 t0rx -- Allow user to specify data version with an attribute
            return RegisterType(type, name, surrogateFactory, dataVersion);
        }

        public void RegisterType(Type type, string name)
        {
            RegisterType(type, name, 0);
        }

        public void RegisterType(Type type, string name, Func<FudgeContext, IFudgeSerializationSurrogate> surrogateFactory)
        {
            RegisterType(type, name, surrogateFactory, 0);
        }

        /// <summary>
        /// Registers a type with a serialization surrogate that needs no internal state.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="statelessSurrogate"></param>
        public void RegisterType(Type type, string name, IFudgeSerializationSurrogate statelessSurrogate)
        {
            RegisterType(type, name, c => statelessSurrogate, 0);
        }

        public void RegisterType(Type type, string name, int typeVersion)
        {
            if (typeof(IFudgeSerializable).IsAssignableFrom(type))
            {
                // Class implements IFudgeSerializable directly, so manufacture a surrogate (we only need one as it maintains no state)
                var surrogate = new SerializableSurrogate(type);
                RegisterType(type, name, context => surrogate, typeVersion);
            }
        }

        private int RegisterType(Type type, string name, Func<FudgeContext, IFudgeSerializationSurrogate> surrogateFactory, int typeVersion)
        {
            // TODO 2009-10-18 t0rx -- Handle IFudgeSerializable
            int id = typeDataList.Count;
            var entry = new TypeData { Name = name, SurrogateFactory = surrogateFactory, Type = type };
            typeDataList.Add(entry);
            nameMap.Add(name, id);
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
                return AutoRegister(type);
            }

            return -1;
        }

        public IList<string> GetTypeNames()
        {
            return typeDataList.ConvertAll(entry => entry.Name);
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
                int typeId = AutoRegister(type);
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

        public string GetTypeName(int typeId)
        {
            return typeDataList[typeId].Name;
        }

        /// <summary>
        /// Remap this type map into a new one based on a different ordering/set of type names.
        /// </summary>
        /// <param name="names"></param>
        /// <param name="typeVersions"></param>
        /// <returns></returns>
        public SerializationTypeMap Remap(string[] names, int[] typeVersions)
        {
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }
            if (typeVersions != null && names.Length != typeVersions.Length)
            {
                throw new ArgumentOutOfRangeException("typeVersions", "Lengths of names and type versions must match");
            }

            SerializationTypeMap result = new SerializationTypeMap(context);
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                int index;
                if (nameMap.TryGetValue(name, out index))
                {
                    var data = typeDataList[index];
                    if (typeVersions == null)
                        result.RegisterType(data.Type, name, data.SurrogateFactory, typeVersions[i]);
                    else
                        result.RegisterType(data.Type, name, data.SurrogateFactory);
                }
                else
                {
                    // Unknown
                    // TODO 2009-10-18 t0rx -- Handling for unknown types
                    result.RegisterType(null, name, c => null);
                }
            }
            return result;
        }

        private class TypeData
        {
            public Type Type { get; set; }
            public string Name { get; set; }
            public Func<FudgeContext, IFudgeSerializationSurrogate> SurrogateFactory { get; set; }
        }
    }
}
