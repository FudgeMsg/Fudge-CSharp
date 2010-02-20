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
    /// <summary>
    /// Provides a mapping from .net types to their equivalent Java class names.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To specify a type mapper other than the default, either set this in <see cref="FudgeSerializer.TypeMappingStrategy"/>
    /// or through <see cref="FudgeContext.SetProperty"/> using <see cref="FudgeSerializer.TypeMappingStrategyProperty"/>.
    /// </para>
    /// <para>
    /// The following rules are used:
    /// <list type="bullet">
    /// <item><description>Java package names are lower case</description></item>
    /// <item><description>The initial portion of a Java package name and a .net namespace may be different</description></item>
    /// <item><description>Nested classes are demarked with <c>$</c> in Java but <c>+</c> in .net</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows a <see cref="JavaTypeMappingStrategy"/> being constructed that maps between the
    /// <c>Fudge</c> .net namespace and the <c>org.fudgemsg</c> Java package, and registering it as the
    /// default to use for all <see cref="FudgeSerializer"/>s created from the context:
    /// <code>
    /// var context = new FudgeContext();
    /// var mapper = new JavaTypeMappingStrategy("Fudge.Tests.Unit", "org.fudgemsg");
    /// context.SetProperty(FudgeSerializer.TypeMappingStrategyProperty, mapper);
    /// </code>
    /// </example>
    public class JavaTypeMappingStrategy : DefaultTypeMappingStrategy
    {
        private readonly string dotNetPrefix;
        private readonly string javaPrefix;

        /// <summary>
        /// Constructs a new <see cref="JavaTypeMappingStrategy"/> where the .net namespace maps directly onto the Java package.
        /// </summary>
        public JavaTypeMappingStrategy()
            : this("", "")
        {
        }

        /// <summary>
        /// Constructs a new <see cref="JavaTypeMappingStrategy"/>, mapping between a given .net namespace prefix and Java package prefix.
        /// </summary>
        /// <param name="dotNetPrefix">Initial portion of .net namespace that needs to be swapped with the Java equivalent.</param>
        /// <param name="javaPrefix">Initial portion of Java package name that needs to be swapped with the .net equivalent.</param>
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
            string newTail = string.Join(".", parts.Select(s => s.ToLower()).Concat(new string[] { last }).ToArray());
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
