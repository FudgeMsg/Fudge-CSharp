/* <!--
 * Copyright (C) 2009 - 2009 by OpenGamma Inc. and other contributors.
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

namespace Fudge
{
    /// <summary>
    /// <c>Field</c> is a convenience class to allow functional construction of messages.
    /// </summary>
    /// <example>
    /// The following example shows constructing a message containing two sub-messages:
    /// <code>
    /// var inputMsg = new FudgeMsg(   
    ///                     new Field("sub1",
    ///                         new Field("bibble", "fibble"),
    ///                         new Field(827, "Blibble")),
    ///                     new Field("sub2", 
    ///                         new Field("bibble9", 9837438),
    ///                         new Field(828, 82.77f)));
    /// </code>
    /// </example>
    public class Field : IFudgeField
    {
        private readonly object value;
        private readonly FudgeFieldType type;
        private readonly short? ordinal;
        private readonly string name;
        private static readonly FudgeContext emptyContext = new FudgeContext();

        /// <summary>
        /// Constructs a field with a name and value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public Field(string name, object value) : this(name, null, value)
        {
        }

        /// <summary>
        /// Constructs a field with an ordinal and value.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="value"></param>
        public Field(int ordinal, object value) : this(null, ordinal, value)
        {
        }

        /// <summary>
        /// Constructs a named field that contains a sub-message of other fields.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="subFields"></param>
        /// <example>
        /// The following example shows a hierarchical message being created using <see cref="Field"/>.
        /// <code>
        /// FudgeMsg inputMsg = context.NewMessage(
        ///            new Field("sub1",
        ///                new Field("bibble", "fibble"),
        ///                new Field(827, "Blibble")),
        ///            new Field("sub2",
        ///                new Field("bibble9", 9837438),
        ///                new Field(828, 82.77f)));
        /// 
        /// </code>
        /// </example>
        public Field(string name, params IFudgeField[] subFields) : this(name, null, new FudgeMsg(subFields))
        {
        }

        /// <summary>
        /// Constructs a field with both a name and an ordinal plus value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ordinal"></param>
        /// <param name="value"></param>
        public Field(string name, int? ordinal, object value)
        {
            if (ordinal.HasValue && ordinal < short.MinValue || ordinal > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException("ordinal", "Ordinal must be within signed 16-bit range");
            }
            this.name = name;
            this.ordinal = (short?)ordinal;
            this.value = value;
            this.type = emptyContext.TypeHandler.DetermineTypeFromValue(value);
        }

        #region IFudgeField Members

        /// <inheritdoc/>
        public FudgeFieldType Type
        {
            get { return type; }
        }

        /// <inheritdoc/>
        public object Value
        {
            get { return value; }
        }

        /// <inheritdoc/>
        public short? Ordinal
        {
            get { return ordinal; }
        }

        /// <inheritdoc/>
        public string Name
        {
            get { return name ; }
        }

        #endregion
    }
}
