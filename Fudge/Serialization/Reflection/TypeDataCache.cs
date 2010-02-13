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
using System.Threading;
using System.Diagnostics;

namespace Fudge.Serialization.Reflection
{
    /// <summary>
    /// <c>TypeDataCache</c> is used to cache <see cref="TypeData"/> reflected from types for serialisation.
    /// </summary>
    public class TypeDataCache
    {
        // TODO 2010-02-13 t0rx -- Should TypeDataCache be collapsed into SerializationTypeMap?
        private readonly FudgeContext context;
        private readonly Dictionary<Type, TypeData> cache = new Dictionary<Type, TypeData>();

        public TypeDataCache(FudgeContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            this.context = context;
        }

        public TypeData GetTypeData(Type type, FudgeFieldNameConvention fieldNameConvention)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            TypeData result;
            lock (cache)
            {
                if (!cache.TryGetValue(type, out result))
                {
                    result = new TypeData(context, this, type, fieldNameConvention);
                    Debug.Assert(cache.ContainsKey(type));                // TypeData registers itself during construction
                }
            }
            return result;
        }

        internal void RegisterTypeData(TypeData data)
        {
            lock (cache)
            {
                Debug.Assert(!cache.ContainsKey(data.Type));
                cache.Add(data.Type, data);
            }
        }
    }
}
