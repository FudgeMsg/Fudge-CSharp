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
    /// Overrides the name of the field that a property will serialize as.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FudgeFieldNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FudgeFieldName"/> class.
        /// </summary>
        /// <param name="name">Field name to use for this property.</param>
        public FudgeFieldNameAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the field name to use for this property.
        /// </summary>
        public string Name { get; set; }
    }
}
