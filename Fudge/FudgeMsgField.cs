/**
* Copyright (C) 2009 - 2009 by OpenGamma Inc.
*
* Please see distribution for license.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge.Taxon;

namespace OpenGamma.Fudge
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

        public FudgeMsgField(IFudgeField field)
            : this(field.Type, field.Value, field.Name, field.Ordinal)
        {
        }

        #region IFudgeField Members

        public FudgeFieldType Type
        {
            get { return type; }
        }

        public object Value
        {
            get { return value; }
        }

        public short? Ordinal
        {
            get { return ordinal; }
        }

        public string Name
        {
            get { return name; }
        }

        #endregion

        #region ICloneable Members

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

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
    }
}
