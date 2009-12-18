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
 * -->
 */
using System;
using System.Collections.Generic;
using System.Text;
using Fudge.Taxon;

namespace Fudge
{
    /// <summary>
    /// A concrete implementation of <see cref="IFudgeField"/> suitable for inclusion in
    /// a pre-constructed <see cref="FudgeMsg"/> or a stream of data.
    /// </summary>
    public class FudgeMsgField : FudgeEncodingObject, IFudgeField, ICloneable
    {
        private readonly FudgeFieldType type;
        private readonly object value;
        private readonly string name;
        private readonly short? ordinal;

        /// <summary>
        /// Creates a new message field.
        /// </summary>
        /// <param name="type">field data type</param>
        /// <param name="value">field value</param>
        /// <param name="name">field name, or null for no field name</param>
        /// <param name="ordinal">ordinal index, or null for no ordinal index</param>
        public FudgeMsgField(FudgeFieldType type, object value, string name, short? ordinal)
        {
            if (type == null)
            {
                throw new ArgumentNullException("Must specify a type for this field.");
            }
            this.type = type;
            this.value = value;
            this.name = name;
            this.ordinal = ordinal;
        }

        /// <summary>
        /// Creates a new message field from an existing field.
        /// </summary>
        /// <param name="field">an existing field to copy</param>
        public FudgeMsgField(IFudgeField field)
            : this(field.Type, field.Value, field.Name, field.Ordinal)
        {
        }

        #region IFudgeField Members

        /// <summary>
        /// Gets the type of this field.
        /// </summary>
        public FudgeFieldType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the value of this field.
        /// </summary>
        public object Value
        {
            get { return value; }
        }

        /// <summary>
        /// Gets the ordinal index of this field. This has value null if no ordinal index is specified.
        /// </summary>
        public short? Ordinal
        {
            get { return ordinal; }
        }

        /// <summary>
        /// Gets the descriptive name of this field. This has value null if no name is specified, e.g. if a message has not been resolved against a taxonomy.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        #endregion

        #region ICloneable Members

        /// <inheritdoc cref="System.Object.Clone()" />
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        /// <inheritdoc cref="System.Object.ToString()" />
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Field[");
            if (name != null)
            {
                sb.Append(name);
                if (ordinal == null)
                {
                    sb.Append(":");
                }
                else
                {
                    sb.Append(",");
                }
            }
            if (ordinal != null)
            {
                sb.Append(ordinal).Append(":");
            }

            sb.Append(type);
            sb.Append("-").Append(value);
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Calculates the encoded size of this field in bytes. This is the encoded size of the
        /// underlying value as defined by the corresponding <c>FudgeFieldType</c>, the 2 byte
        /// field prefix, plus the ordinal index and field name if specified. If a taxonomy is
        /// specified and defines the field name, only the corresponding ordinal index would be
        /// written so the field name is not counted.
        /// </summary>
        /// <param name="taxonomy">taxonomy used to resolve field names, or null</param>
        /// <returns>the encoded size of this field in bytes</returns>
        public override int ComputeSize(IFudgeTaxonomy taxonomy)
        {
            int size = 0;
            // Field prefix
            size += 2;
            bool hasOrdinal = ordinal != null;
            bool hasName = name != null;
            if ((name != null) && (taxonomy != null))
            {
                if (taxonomy.GetFieldOrdinal(name) != null)
                {
                    hasOrdinal = true;
                    hasName = false;
                }
            }
            if (hasOrdinal)
            {
                size += 2;
            }
            if (hasName)
            {
                // One for the size prefix
                size++;
                // Then for the UTF Encoding
                size += ModifiedUTF8Util.ModifiedUTF8Length(name);
            }
            if (type.IsVariableSize)
            {
                int valueSize = type.GetVariableSize(value, taxonomy);
                if (valueSize <= 255)
                {
                    size += valueSize + 1;
                }
                else if (valueSize <= short.MaxValue)
                {
                    size += valueSize + 2;
                }
                else
                {
                    size += valueSize + 4;
                }
            }
            else
            {
                size += type.FixedSize;
            }
            return size;
        }

        /// <summary>
        /// Helper function for converting to a base interface to satisfy C# type checking rules on collections. Can be used, for
        /// example, to turn a List&lt;FudgeMsgField&gt; into a List&lt;IFudgeField&gt; using the ConvertAll method on List.
        /// </summary>
        /// <param name="f">a FudgeMsgField object</param>
        /// <returns>a IFudgeField object</returns>
        public static IFudgeField toIFudgeField(FudgeMsgField f)
        {
            return (IFudgeField)f;
        }
    }
}
