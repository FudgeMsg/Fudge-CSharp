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
using System.IO;
using Fudge.Types;

namespace Fudge
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

        /// <summary>
        /// Creates a new <c>FudgeFieldType</c> mapped to an underlying .NET type that can hold the data.
        /// </summary>
        /// <param name="typeId">the numeric identifier to use when encoding this type, must be between 0 and 255</param>
        /// <param name="csharpType">an underlying .NET type to hold the field data</param>
        /// <param name="isVariableSize">set to true if the type can't be encoded with a fixed width</param>
        /// <param name="fixedSize">if the field can be encoded with a fixed width, the number of bytes required to encode it</param>
        public FudgeFieldType(int typeId, Type csharpType, bool isVariableSize, int fixedSize)
        {
            if (csharpType == null)
            {
                throw new ArgumentNullException("Must specify a valid CSharp type for conversion.");
            }
            if ((typeId < 0) || (typeId > 255))
            {
                throw new ArgumentOutOfRangeException("The type id must fit in an unsigned byte.");
            }
            this.typeId = typeId;
            this.csharpType = csharpType;
            this.isVariableSize = isVariableSize;
            this.fixedSize = fixedSize;

            this.toStringValue = GenerateToString();
        }

        /// <summary>
        /// Gets the numeric identifier assigned to this type
        /// </summary>
        public int TypeId
        {
            get { return typeId; }
        }

        /// <summary>
        /// Gets the underlying .NET type 
        /// </summary>
        public Type CSharpType
        {
            get
            {
                return csharpType;
            }
        }
        // TODO 2009-11-14 Andrew -- should we have 'translation' wrappers to make code a bit more readable when working with the other .NET languages, e.g. a VBType one?

        /// <summary>
        /// Gets whether the field can have variable length. If this attribute is false the field can be encoded with a fixed width.
        /// </summary>
        public bool IsVariableSize
        {
            get
            {
                return isVariableSize;
            }
        }

        /// <summary>
        /// Gets the size in bytes of a fixed width field.
        /// </summary>
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
        /// Converts a value of another type to one of this field type, if possible.
        /// </summary>
        /// <remarks>
        /// Override this to provide custom conversions.  The default behaviour is to use the default .net conversions.
        /// </remarks>
        /// <param name="value">Value to convert.</param>
        /// <returns>Converted value.</returns>
        /// <exception cref="InvalidCastException">Thrown if the value cannot be converted</exception>
        public virtual object ConvertValueFrom(object value)
        {
            // TODO 2009-09-12 t0rx -- Should we return null rather than throwing an exception?  This would be consistent with FudgeMsg.GetLong, etc.
            // TODO 2009-12-14 Andrew -- I think throwing an exception is correct behaviour; FudgeMsg.GetLong might be flawed!

            return Convert.ChangeType(value, csharpType);
        }

        /// <summary>
        /// Tests if this object is equal to another. Two <c>FudgeFieldType</c>s are equal iff they have the same numeric type identifier.
        /// </summary>
        /// <param name="obj">the object to compare to</param>
        /// <returns>true iff the objects are equal</returns>
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

        /// <inheritdoc cref="System.Object.GetHashCode()" />
        public override int GetHashCode()
        {
            return TypeId;
        }

        /// <summary>
        /// Returns a string representation of this type. Override this in preference to <c>ToString</c> as it
        /// will only be called once at object construction time. The default <c>ToString</c> method returns
        /// this cached value.
        /// </summary>
        /// <returns>string representation</returns>
        protected virtual string GenerateToString()
        {
            string str = string.Format("FudgeFieldType[{0}-{1}]", TypeId, CSharpType);
            return string.Intern(str);
        }

        /// <inheritdoc cref="System.Object.ToString()" />
        public override string ToString()
        {
            return toStringValue;
        }

        /// <summary>
        /// Calculates the size of an encoded value. This method must be provided for any variable size types.
        /// </summary>
        /// <param name="value">the value to calculate the size for</param>
        /// <param name="taxonomy">the taxonomy to encode against</param>
        /// <returns>the size in bytes of the encoded value</returns>
        public abstract int GetVariableSize(object value, IFudgeTaxonomy taxonomy);

        /// <summary>
        /// Writes an encoded value. The output must contain only the value data, no header or prefix is required.
        /// If the type is a variable size, the number of bytes written must be equal to the value returned by
        /// <c>GetVariableSize</c> or the resulting message will not be valid.
        /// </summary>
        /// <param name="output">the target to write to</param>
        /// <param name="value">the value to write</param>
        /// <param name="taxonomy"></param>
        public abstract void WriteValue(BinaryWriter output, object value, IFudgeTaxonomy taxonomy);

        /// <summary>
        /// Reads an encoded value. The input contains only the value data, no header or prefix is available. The
        /// reader must read the full number of bytes available. Reading too few or too many may result in subsequent
        /// reads to fail or the message to be corrupted.
        /// </summary>
        /// <param name="input">the source to read from</param>
        /// <param name="dataSize">the number of bytes available for the value</param>
        /// <param name="typeDictionary"></param>
        /// <returns>the decoded value</returns>
        public abstract object ReadValue(BinaryReader input, int dataSize, FudgeTypeDictionary typeDictionary);

        // TODO 2009-12-14 Andrew -- instead of the TypeDictionary, we should pass the current FudgeContext
        // TODO 2009-12-14 Andrew -- passing in the current taxonomy could be jolly useful too when reading a submessage, or are we happy doing the "fixup" afterwards
        // TODO 2009-12-14 Andrew -- how about a default implementation of these that calls a simpler abstract form for the typical cases which don't require context or taxonomy data?
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
        // TODO 2009-08-30 t0rx -- Is this the best way of handling this - Fudge-Java can use <?> but there's no equivalent in C#...
        // TODO 2009-12-14 Andrew -- there must be a less cumbersome approach than this that doesn't force us to end up with ReadTypedValue

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

        /// <inheritdoc />
        public virtual int GetVariableSize(TValue value, IFudgeTaxonomy taxonomy)
        {
            if (IsVariableSize)
            {
                throw new NotSupportedException("This method must be overridden for variable size types.");
            }
            return FixedSize;
        }

        /// <inheritdoc />
        public virtual void WriteValue(BinaryWriter output, TValue value, IFudgeTaxonomy taxonomy) //throws IOException
        {
            if (IsVariableSize)
            {
                throw new NotSupportedException("This method must be overridden for variable size types.");
            }
        }

        /// <inheritdoc cref="FudgeFieldType.ReadValue(System.IO.BinaryReader,System.Int32,Fudge.FudgeTypeDictionary)" />
        public virtual TValue ReadTypedValue(BinaryReader input, int dataSize, FudgeTypeDictionary typeDictionary) //throws IOException
        {
            // TODO 2009-08-30 t0rx -- In Fudge-Java this is just readValue, but it creates problems here because the parameters are the same as the base's ReadValue
            if (IsVariableSize)
            {
                throw new NotSupportedException("This method must be overridden for variable size types.");
            }
            return default(TValue);
        }

        /// <summary>
        /// Attempts to reduce a value to a more primitive type using the minimizer delegate.
        /// </summary>
        /// <param name="type">value to minimise</param>
        /// <param name="value">type of the value - will be updated if a reduction takes place</param>
        /// <returns>reduced value, or the original value if no reduction is possible</returns>
        public override object Minimize(object value, ref FudgeFieldType type)
        {
            if (minimizer != null)
            {
                return minimizer((TValue)value, ref type);
            }

            return value;
        }

        #region Mapping from untyped into typed method calls
        /// <inheritdoc />
        public sealed override int GetVariableSize(object value, IFudgeTaxonomy taxonomy)
        {
            return GetVariableSize((TValue)value, taxonomy);
        }

        /// <inheritdoc />
        public sealed override void WriteValue(BinaryWriter output, object value, IFudgeTaxonomy taxonomy)
        {
            WriteValue(output, (TValue)value, taxonomy);
        }

        /// <inheritdoc />
        public sealed override object ReadValue(BinaryReader input, int dataSize, FudgeTypeDictionary typeDictionary)
        {
            return ReadTypedValue(input, dataSize, typeDictionary);
        }
        #endregion
    }

}
