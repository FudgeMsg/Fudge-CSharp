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
    public class SerializationTypeMap
    {
        private readonly List<TypeData> typeDataList = new List<TypeData>();
        private readonly Dictionary<string, int> nameMap = new Dictionary<string, int>();
        private readonly Dictionary<Type, int> typeMap = new Dictionary<Type, int>();

        public SerializationTypeMap()
        {
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
        /// <param name="globalSurrogate"></param>
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

        private void RegisterType(Type type, string name, Func<FudgeContext, IFudgeSerializationSurrogate> surrogateFactory, int typeVersion)
        {
            // TODO t0rx 2009-10-18 -- Handle IFudgeSerializable
            int id = typeDataList.Count;
            var entry = new TypeData { Name = name, SurrogateFactory = surrogateFactory, TypeVersion = typeVersion, Type = type };
            typeDataList.Add(entry);
            nameMap.Add(name, id);
            typeMap.Add(type, id);
        }

        public int GetTypeId(Type type)
        {
            int index;
            if (typeMap.TryGetValue(type, out index))
            {
                return index;
            }
            return -1;
        }

        public IList<string> GetTypeNames()
        {
            return typeDataList.ConvertAll(entry => entry.Name);
        }

        public IList<int> GetTypeVersions()
        {
            return typeDataList.ConvertAll(entry => entry.TypeVersion);
        }

        public Func<FudgeContext, IFudgeSerializationSurrogate> GetSurrogateFactory(Type type)
        {
            int index;
            if (typeMap.TryGetValue(type, out index))
            {
                return typeDataList[index].SurrogateFactory;
            }
            return null;
        }

        public Func<FudgeContext, IFudgeSerializationSurrogate> GetSurrogateFactory(int typeId)
        {
            if (typeId < 0 || typeId >= typeDataList.Count)
                return null;

            return typeDataList[typeId].SurrogateFactory;
        }

        public int GetTypeVersion(int typeId)
        {
            return typeDataList[typeId].TypeVersion;
        }

        public string GetTypeName(int typeId)
        {
            return typeDataList[typeId].Name;
        }

        /// <summary>
        /// Remap this type map into a new one based on a different ordering/set of type names.
        /// </summary>
        /// <param name="names"></param>
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

            SerializationTypeMap result = new SerializationTypeMap();
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
                    // TODO t0rx 2009-10-18 -- Handling for unknown types
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
            public int TypeVersion { get; set; }
        }
    }
}
