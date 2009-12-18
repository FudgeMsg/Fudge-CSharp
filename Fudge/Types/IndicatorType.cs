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
using System.Linq;
using System.Text;

namespace Fudge.Types
{
    /// <summary>
    /// The only value of a field with the Indicator type.
    /// </summary>
    public sealed class IndicatorType : IConvertible
    {
        private IndicatorType() { }

        /// <summary>
        /// The only instance of this type.
        /// </summary>
        public static readonly IndicatorType Instance = new IndicatorType();

        #region IConvertible Members

        /// <inheritdoc cref="System.IConvertible.GetTypeCode()" />
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        /// <inheritdoc cref="System.IConvertible.ToBoolean(System.IFormatProvider)" />
        public bool ToBoolean(IFormatProvider provider)
        {
            return false;
        }

        /// <inheritdoc cref="System.IConvertible.ToByte(System.IFormatProvider)" />
        public byte ToByte(IFormatProvider provider)
        {
            return 0;
        }

        /// <inheritdoc cref="System.IConvertible.ToChar(System.IFormatProvider)" />
        public char ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc cref="System.IConvertible.ToDateTime(System.IFormatProvider)" />
        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc cref="System.IConvertible.ToDecimal(System.IFormatProvider)" />
        public decimal ToDecimal(IFormatProvider provider)
        {
            return 0;
        }

        /// <inheritdoc cref="System.IConvertible.ToDouble(System.IFormatProvider)" />
        public double ToDouble(IFormatProvider provider)
        {
            return 0;
        }

        /// <inheritdoc cref="System.IConvertible.ToInt16(System.IFormatProvider)" />
        public short ToInt16(IFormatProvider provider)
        {
            return 0;
        }

        /// <inheritdoc cref="System.IConvertible.ToInt32(System.IFormatProvider)" />
        public int ToInt32(IFormatProvider provider)
        {
            return 0;
        }

        /// <inheritdoc cref="System.IConvertible.ToInt64(System.IFormatProvider)" />
        public long ToInt64(IFormatProvider provider)
        {
            return 0;
        }

        /// <inheritdoc cref="System.IConvertible.ToSByte(System.IFormatProvider)" />
        public sbyte ToSByte(IFormatProvider provider)
        {
            return 0;
        }

        /// <inheritdoc cref="System.IConvertible.ToSingle(System.IFormatProvider)" />
        public float ToSingle(IFormatProvider provider)
        {
            return 0;
        }

        /// <inheritdoc cref="System.IConvertible.ToString(System.IFormatProvider)" />
        public string ToString(IFormatProvider provider)
        {
            return "";
        }

        /// <inheritdoc cref="System.IConvertible.ToType(System.Type,System.IFormatProvider)" />
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc cref="System.IConvertible.ToUInt16(System.IFormatProvider)" />
        public ushort ToUInt16(IFormatProvider provider)
        {
            return 0;
        }

        /// <inheritdoc cref="System.IConvertible.ToUInt32(System.IFormatProvider)" />
        public uint ToUInt32(IFormatProvider provider)
        {
            return 0;
        }

        /// <inheritdoc cref="System.IConvertible.ToUInt64(System.IFormatProvider)" />
        public ulong ToUInt64(IFormatProvider provider)
        {
            return 0;
        }

        #endregion
    }
}
