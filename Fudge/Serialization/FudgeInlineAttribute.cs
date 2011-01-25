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
    /// Specifies whether an object is serialized in-line within its parent or referenced.
    /// </summary>
    /// <remarks>
    /// On a class, this controls the default behaviour for that class, and will override the default.  On a
    /// property, this will override the behaviour for the property type, just for that property.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class FudgeInlineAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of this class, defaulting <see cref="Inline"/> to be <c>true</c>.
        /// </summary>
        public FudgeInlineAttribute()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="inline">If <c>true</c> then the object will be serialized in-line in its parent rather than as a reference.</param>
        public FudgeInlineAttribute(bool inline)
        {
            this.Inline = inline;
        }

        /// <summary>
        /// Gets and sets whether the object will be serialized in-line in its parent rather than as a reference.
        /// </summary>
        public bool Inline
        {
            get;
            set;
        }
    }
}
