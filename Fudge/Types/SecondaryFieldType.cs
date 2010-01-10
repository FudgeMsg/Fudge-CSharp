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
using Fudge.Taxon;
using System.IO;

namespace Fudge
{
    /// <summary>
    /// Marker interface to indicate that this type isn't a primary type
    /// </summary>
    public interface ISecondaryFieldType
    {
    }

    /// <summary>
    /// SecondaryFieldTypes are used to represent <see cref="FudgeFieldType"/>s that are represented as
    /// a more primitive type when encoded.
    /// </summary>
    /// <typeparam name="T">Value type for this field.</typeparam>
    /// <typeparam name="RawType">Type for the more primitive encoded value.</typeparam>
    public class SecondaryFieldType<T, RawType> : FudgeFieldType<T>, ISecondaryFieldType
    {
        private readonly FudgeFieldType wireType;
        private readonly Converter<RawType, T> inputConverter;
        private readonly Converter<T, RawType> outputConverter;

        /// <summary>
        /// Cosntructs a new <c>SeondaryFieldType</c>.
        /// </summary>
        /// <param name="wireType">Canonical type that values of this type will be represented as when encoded.</param>
        /// <param name="inputConverter">Function to convert values from canonical type to this type.</param>
        /// <param name="outputConverter">Function to convert values from this type to the canonical type.</param>
        public SecondaryFieldType(FudgeFieldType wireType, Converter<RawType, T> inputConverter, Converter<T, RawType> outputConverter)
            : base(wireType.TypeId, wireType.IsVariableSize, wireType.FixedSize)
        {
            if (wireType == null) throw new ArgumentNullException("wireType");
            if (inputConverter == null) throw new ArgumentNullException("inputConverter");
            if (outputConverter == null) throw new ArgumentNullException("outputConverter");
            if (wireType.CSharpType != typeof(RawType))
            {
                throw new ArgumentException("wireType does not match RawType for " + GetType().Name);
            }

            this.wireType = wireType;
            this.inputConverter = inputConverter;
            this.outputConverter = outputConverter;
        }

        /// <summary>
        /// Cosntructs a new <c>SeondaryFieldType</c> without conversion functions.
        /// </summary>
        /// <param name="wireType">Canonical type that values of this type will be represented as when encoded.</param>
        /// <remarks>
        /// Derived types can use this form if they wish to override <see cref="Minimize"/> and <see cref="ConvertValueFrom"/>
        /// directly rather than supplying conversion functions.
        /// </remarks>
        protected SecondaryFieldType(FudgeFieldType wireType)
            : base(wireType.TypeId, wireType.IsVariableSize, wireType.FixedSize)
        {
            if (wireType == null) throw new ArgumentNullException("wireType");
            if (wireType.CSharpType != typeof(RawType))
            {
                throw new ArgumentException("wireType does not match RawType for " + GetType().Name);
            }

            // Use overrides instead of delegates
            this.inputConverter = (raw => ConvertFromWire(raw));
            this.outputConverter = (value => ConvertToWire(value));
        }

        #region Overrides for methods that should be handled by the wire type
        /// <inheritdoc cref="Fudge.FudgeFieldType.GetVariableSize(System.Object,Fudge.Taxon.IFudgeTaxonomy)" />
        public override int GetVariableSize(T value, IFudgeTaxonomy taxonomy)
        {
            throw new NotSupportedException("Secondary type should never have to get a value size, the wire type should handle this");
        }

        /// <inheritdoc/>
        public override T ReadTypedValue(BinaryReader input, int dataSize)
        {
            throw new NotSupportedException("Secondary type should never have to read a value, the wire type should handle this");
        }

        /// <inheritdoc/>
        public override void WriteValue(BinaryWriter output, T value)
        {
            throw new NotSupportedException("Secondary type should never have to write a value, the wire type should handle this");
        }
        #endregion

        /// <inheritdoc/>
        public override object Minimize(object value, ref FudgeFieldType type)
        {
            type = wireType;
            object newValue = outputConverter((T)value);
            return wireType.Minimize(newValue, ref type);       // Allow the wire type to minimise further if it wants
        }

        /// <inheritdoc/>
        public override object ConvertValueFrom(object value)
        {
            if (value is RawType)
            {
                return inputConverter((RawType)value);
            }

            return base.ConvertValueFrom(value);
        }

        /// <summary>
        /// Override ConvertToWire if you do not wish to use a delegate.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual RawType ConvertToWire(T value)
        {
            throw new NotSupportedException("You must override SecondaryFieldType<" + typeof(T).Name + ">.ConvertToWire if you do not pass converters to the constructor.");
        }

        /// <summary>
        /// Override ConvertFromWire if you do not wish to use a delegate.
        /// </summary>
        /// <param name="rawValue"></param>
        /// <returns></returns>
        protected virtual T ConvertFromWire(RawType rawValue)
        {
            throw new NotSupportedException("You must override SecondaryFieldType<" + typeof(T).Name + ">.ConvertFromWire if you do not pass converters to the constructor.");
        }
    }
}
