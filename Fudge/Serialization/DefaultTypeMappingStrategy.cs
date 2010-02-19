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

namespace Fudge.Serialization
{
    /// <summary>
    /// <c>DefaultTypeMappingStrategy</c> maps a <see cref="Type"/> onto its full name, and
    /// scans all loaded assemblies to try to find a type from its name.
    /// </summary>
    public class DefaultTypeMappingStrategy : IFudgeTypeMappingStrategy
    {
        private readonly Dictionary<string, Type> typeMap = new Dictionary<string, Type>();
        
        #region IFudgeTypeMappingStrategy Members

        /// <inheritdoc/>
        public virtual string GetName(Type type)
        {
            return type.FullName;
        }

        /// <inheritdoc/>
        public virtual Type GetType(string name)
        {
            return GetCachedType(name, false);
        }

        #endregion

        /// <summary>
        /// Uses <see cref="FindType"/> to find a type from a name, then caches it.
        /// </summary>
        /// <param name="name">Name of type to get.</param>
        /// <param name="ignoreCase">If <c>true</c> then case is ignored.</param>
        /// <returns>Matching <see cref="Type"/>, or <c>null</c> if not found.</returns>
        protected Type GetCachedType(string name, bool ignoreCase)
        {
            string key = ignoreCase ? name.ToLower() : name;

            Type result;
            if (!typeMap.TryGetValue(key, out result))
            {
                result = FindType(name, ignoreCase);
                if (result != null)
                {
                    typeMap[key] = result;
                }
            }
            return result;
        }

        /// <summary>
        /// Performs a search of all the assemblies in the current <see cref="AppDomain"/> to find the given type.
        /// </summary>
        /// <param name="name">Name of type to find.</param>
        /// <param name="ignoreCase">If <c>true</c> then case is ignored when searching for types.</param>
        /// <returns>Type if found or <c>null</c>.</returns>
        /// <remarks>Derived types can override this method to change the search behaviour.</remarks>
        protected virtual Type FindType(string name, bool ignoreCase)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type result = assembly.GetType(name, false, ignoreCase);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
