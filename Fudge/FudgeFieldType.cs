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
using OpenGamma.Fudge.Types;

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

        /// <summary>
        /// Override <c>Minimize</c> where you wish to be able to reduce to a lower type as fields are added to messages.
        /// </summary>
        /// <remarks>
        /// <c>Minimize</c> is used to reduce integers to their smallest form, but can also be used to convert
        /// non-primitive types (e.g. GUID) to primitive ones (e.g. byte[16]) as they are added.
        /// </remarks>
        /// <param name="value">Value to reduce.</param>
        /// <param name="type">Current field type - update this if you minimize to a different type.</param>
        /// <returns>Minimized value.</returns>
        public virtual object Minimize(object value, ref FudgeFieldType type)
        {
            return value;
        }

        /// <summary>
        /// Converts a value of another type to on of this field type, if possible.
        /// </summary>
        /// <remarks>
        /// Override this to provide custom conversions.  The default behaviour is to use the default .net conversions.
        /// </remarks>
        /// <param name="value">Value to convert.</param>
        /// <returns>Converted value.</returns>
        /// <exception cref="InvalidCastException">Thrown if the value cannot be converted</exception>
        public virtual object ConvertValueFrom(object value)
        {
            // TODO t0rx 2009-09-12 -- Should we return null rather than throwing an exception?  This would be consistent with FudgeMsg.GetLong, etc.

            return Convert.ChangeType(value, csharpType);
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

        protected virtual string GenerateToString()
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

        public abstract object ReadValue(BinaryReader input, int dataSize);
    }

    /// <summary>
    /// <c>FudgeValueMinimizer</c> is used to reduce values to their most primitive form for encoding
    /// </summary>
    /// <remarks>
    /// The minimizer may simply return the original value.  If it converts to another type then it should update the <c>type</c> parameter also.
    /// </remarks>
    /// <typeparam name="T">Type of value to minimize.</typeparam>
    /// <param name="value">Value to minimize.</param>
    /// <param name="type">Current <see cref="FudgeFieldType"/>, which may be updated by the minimizer.</param>
    /// <returns>Minimized value.</returns>
    public delegate object FudgeValueMinimizer<T>(T value, ref FudgeFieldType type);

    /// <summary>Unlike in Fudge-Java, here we have to have a generic type inheriting from the non-generic base type, as C# doesn't support MyClass&lt;?&gt;</summary>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class FudgeFieldType<TValue> : FudgeFieldType
    {
        // TODO t0rx 2009-08-30 -- Is this the best way of handling this - Fudge-Java can use <?> but there's no equivalent in C#...

        private readonly FudgeValueMinimizer<TValue> minimizer;

        public FudgeFieldType(int typeId, bool isVariableSize, int fixedSize)
            : this(typeId, isVariableSize, fixedSize, null)
        {
        }

        public FudgeFieldType(int typeId, bool isVariableSize, int fixedSize, FudgeValueMinimizer<TValue> minimizer)
            : base(typeId, typeof(TValue), isVariableSize, fixedSize)
        {
            this.minimizer = minimizer;
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

        public virtual TValue ReadTypedValue(BinaryReader input, int dataSize) //throws IOException
        {
            // TODO t0rx 2009-08-30 -- In Fudge-Java this is just readValue, but it creates problems here because the parameters are the same as the base's ReadValue
            if (IsVariableSize)
            {
                throw new NotSupportedException("This method must be overridden for variable size types.");
            }
            return default(TValue);
        }

        public override object Minimize(object value, ref FudgeFieldType type)
        {
            if (minimizer != null)
            {
                return minimizer((TValue)value, ref type);
            }

            return value;
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

        public sealed override object ReadValue(BinaryReader input, int dataSize)
        {
            return ReadTypedValue(input, dataSize);
        }
        #endregion
    }

}
