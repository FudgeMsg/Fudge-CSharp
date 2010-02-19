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
    public class JavaTypeMappingStrategy : DefaultTypeMappingStrategy
    {
        private readonly string dotNetPrefix;
        private readonly string javaPrefix;

        public JavaTypeMappingStrategy(string dotNetPrefix, string javaPrefix)
        {
            if (dotNetPrefix == null)
                throw new ArgumentNullException("dotNetPrefix");
            if (javaPrefix == null)
                throw new ArgumentNullException("javaPrefix");

            if (!dotNetPrefix.EndsWith("."))
                dotNetPrefix = dotNetPrefix + ".";
            this.dotNetPrefix = dotNetPrefix;
            if (!javaPrefix.EndsWith("."))
                javaPrefix = javaPrefix + ".";
            this.javaPrefix = javaPrefix;
        }

        #region IFudgeTypeMappingStrategy Members

        /// <inheritdoc/>
        public override string GetName(Type type)
        {
            string name = base.GetName(type);

            // Split into package and class name
            string tail = name.StartsWith(dotNetPrefix) ? name.Substring(dotNetPrefix.Length) : name;
            var parts = new List<string>(tail.Split('.'));            
            string last = parts[parts.Count - 1];
            parts.RemoveAt(parts.Count - 1);

            // Handle inner classes
            last = last.Replace('+', '$');

            // Convert package name to lower-case and add back in class name
            string newTail = string.Join(".", parts.Select(s => s.ToLower()).Concat(new string[]{last}).ToArray());
            return javaPrefix + newTail;
        }

        /// <inheritdoc/>
        public override Type GetType(string name)
        {
            string tail = name.StartsWith(javaPrefix) ? name.Substring(javaPrefix.Length) : name;
            tail = tail.Replace('$', '+');
            string newName = dotNetPrefix + tail;
            return base.GetCachedType(newName, true);
        }

        #endregion
    }
}
