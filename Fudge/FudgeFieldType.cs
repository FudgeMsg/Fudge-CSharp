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
using System.IO;

namespace OpenGamma.Fudge
{
    /// <summary>
    /// The class defining the type of a particular field.
    /// While Fudge comes with a set of required types which are fully supported
    /// in all Fudge-compliant systems, if you have custom data, you can control the encoding
    /// using your own instance of <see cref="FudgeFieldType"/>, making sure to register the
    /// instance with the <see cref="FudgeTypeDictionary"/> at application startup.
    /// 
    /// </summary>
    [Serializable]
    public abstract class FudgeFieldType
    {
        private readonly int typeId;
        private readonly Type csharpType;
        private readonly bool isVariableSize;
        private readonly int fixedSize;

        private readonly string toStringValue;

        public FudgeFieldType(int typeId, Type csharpType, bool isVariableSize, int fixedSize)
        {
            if (csharpType == null)
            {
                throw new ArgumentNullException("Must specify a valid CSharp type for conversion.");
            }
            if (typeId > 255)
            {
                throw new ArgumentOutOfRangeException("The type id must fit in an unsigned byte.");
            }
            this.typeId = typeId;
            this.csharpType = csharpType;
            this.isVariableSize = isVariableSize;
            this.fixedSize = fixedSize;

            this.toStringValue = GenerateToString();
        }

        public int TypeId
        {
            get { return typeId; }
        }

        public Type CSharpType
        {
            get
            {
                return csharpType;
            }
        }

        public bool IsVariableSize
        {
            get
            {
                return isVariableSize;
            }
        }

        public int FixedSize
        {
            get
            {
                return fixedSize;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            FudgeFieldType other = obj as FudgeFieldType;
            if (other == null)
            {
                return false;
            }
            if (TypeId != other.TypeId)
            {
                return false;
            }

            // Don't bother checking the rest of it.
            return true;
        }

        public override int GetHashCode()
        {
            return TypeId;
        }

        protected string GenerateToString()
        {
            string str = string.Format("FudgeFieldType[{0}-{1}]", TypeId, CSharpType);
            return string.Intern(str);
        }

        public override string ToString()
        {
            return toStringValue;
        }

        public abstract int GetVariableSize(object value, IFudgeTaxonomy taxonomy);

        public abstract void WriteValue(BinaryWriter output, object value, IFudgeTaxonomy taxonomy, short taxonomyId);

        public abstract object ReadValue(BinaryReader input, int dataSize, IFudgeTaxonomy taxonomy);
    }

    /// <summary>Unlike in Fudge-Java, here we have to have a generic type inheriting from the non-generic base type, as C# doesn't support MyClass<?></summary>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class FudgeFieldType<TValue> : FudgeFieldType
    {
        // TODO: 20090830 (t0rx): Is this the best way of handling this - Fudge-Java can use <?> but there's no equivalent in C#...

        public FudgeFieldType(int typeId, bool isVariableSize, int fixedSize) : base(typeId, typeof(TValue), isVariableSize, fixedSize)
        {
        }

        public virtual int GetVariableSize(TValue value, IFudgeTaxonomy taxonomy)
        {
            if (IsVariableSize)
            {
                throw new NotSupportedException("This method must be overridden for variable size types.");
            }
            return FixedSize;
        }

        public virtual void WriteValue(BinaryWriter output, TValue value, IFudgeTaxonomy taxonomy, short taxonomyId) //throws IOException
        {
            if (IsVariableSize)
            {
                throw new NotSupportedException("This method must be overridden for variable size types.");
            }
        }

        public virtual TValue ReadTypedValue(BinaryReader input, int dataSize, IFudgeTaxonomy taxonomy) //throws IOException
        {
            // TODO: 20090830 (t0rx): In Fudge-Java this is just readValue, but it creates problems here because the parameters are the same as the base's ReadValue
            if (IsVariableSize)
            {
                throw new NotSupportedException("This method must be overridden for variable size types.");
            }
            return default(TValue);
        }

        #region Mapping from untyped into typed method calls
        public sealed override int GetVariableSize(object value, IFudgeTaxonomy taxonomy)
        {
            return GetVariableSize((TValue)value, taxonomy);
        }

        public sealed override void WriteValue(BinaryWriter output, object value, IFudgeTaxonomy taxonomy, short taxonomyId)
        {
            WriteValue(output, (TValue)value, taxonomy, taxonomyId);
        }

        public sealed override object ReadValue(BinaryReader input, int dataSize, IFudgeTaxonomy taxonomy)
        {
            return ReadTypedValue(input, dataSize, taxonomy);
        }
        #endregion
    }

}
