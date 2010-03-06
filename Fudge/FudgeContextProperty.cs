/*
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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Fudge
{
    /// <summary>
    /// Identifies a property within a <see cref="FudgeContext"/>.
    /// </summary>
    /// <remarks>
    /// <para>Properties are ussed to control encoding and decoding behaviour.  They are set and retrieved
    /// through the <see cref="FudgeContext.SetProperty"/> and <see cref="FudgeContext.GetProperty(FudgeContextProperty)"/>
    /// methods of <see cref="FudgeContext"/>.</para>
    /// <para>The validation function can be specified as a parameter to the constructor, or by deriving
    /// from this class and overriding <see cref="IsValidValue"/>.</para>
    /// </remarks>
    public class FudgeContextProperty
    {
        private static int nextIndex = -1;
        private readonly int index;
        private readonly string name;
        private readonly Func<object, bool> validator;

        /// <summary>
        /// Construct a property with a given name and no validation of values.
        /// </summary>
        /// <param name="name">Name of the property (used for exceptions).</param>
        public FudgeContextProperty(string name) : this(name, x => true)
        {
        }

        /// <summary>
        /// Cosntruct a property with a given name and validation function.
        /// </summary>
        /// <param name="name">Name of the property (used for exceptions).</param>
        /// <param name="validator">Delegate to use to validate values.</param>
        public FudgeContextProperty(string name, Func<object, bool> validator)
        {
            this.name = name;
            this.validator = validator;
            this.index = Interlocked.Increment(ref nextIndex);            
        }

        /// <summary>
        /// Construct a property with a given name that only requires values to be of a specific type (or specialisation thereof).
        /// </summary>
        /// <param name="name">Name of the property (used for exceptions).</param>
        /// <param name="type">Type that values must be.</param>
        public FudgeContextProperty(string name, Type type) : this(name, x => type.IsAssignableFrom(x.GetType()))
        {            
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Tests the value to ensure that it is valid for this property.
        /// </summary>
        /// <remarks>
        /// By default, this will call the validator delegate (if any) given to the constructor,
        /// but may alternatively be overridden in a derived class instead.
        /// </remarks>
        /// <param name="value">Value to test.</param>
        /// <returns><c>true</c> if the value is valid.</returns>
        public virtual bool IsValidValue(object value)
        {
            return validator(value);
        }

        internal int Index
        {
            get { return index; }
        }

        internal static int MaxIndex
        {
            get { return nextIndex; }       // Note that 32-bit integer reads are always atomic
        }
    }
}
