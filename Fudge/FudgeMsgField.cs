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
    public class FudgeMsgField : FudgeEncodingObject, IFudgeField
    {
        // TODO t0rx 2009-08-30 -- Finish porting FudgeMsgField
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

        ///**
        // * A concrete implementation of {@link FudgeField} suitable for inclusion in
        // * a pre-constructed {@link FudgeMsg} or a stream of data.
        // *
        // * @author kirk
        // */
        //public class FudgeMsgField implements FudgeField, Serializable, Cloneable, SizeComputable {
        //  @SuppressWarnings("unchecked")

        //  @Override
        //  public FudgeMsgField clone() {
        //    Object cloned;
        //    try {
        //      cloned = super.clone();
        //    } catch (CloneNotSupportedException e) {
        //      throw new RuntimeException("This can't happen.");
        //    }
        //    return (FudgeMsgField) cloned;
        //  }

        //  @Override
        //  public String toString() {
        //    StringBuilder sb = new StringBuilder();
        //    sb.append("Field[");
        //    if(_name != null) {
        //      sb.append(_name);
        //      if(_ordinal == null) {
        //        sb.append(":");
        //      } else {
        //        sb.append(",");
        //      }
        //    }
        //    if(_ordinal != null) {
        //      sb.append(_ordinal).append(":");
        //    }

        //    sb.append(_type);
        //    sb.append("-").append(_value);
        //    sb.append("]");
        //    return sb.toString();
        //  }

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
