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
using Fudge.Taxon;
using System.IO;

namespace Fudge.Types
{
    /// <summary>
    /// The type definition for an indicator type - a field type which carries no data.
    /// </summary>
    public class IndicatorFieldType : FudgeFieldType<IndicatorType>
    {
        /// <summary>
        /// A type definition for the indicator type.
        /// </summary>
        public static readonly IndicatorFieldType Instance = new IndicatorFieldType();

        /// <summary>
        /// Creates a new type definition for the indicator type.
        /// </summary>
        public IndicatorFieldType()
            : base(FudgeTypeDictionary.INDICATOR_TYPE_ID, false, 0)
        {
        }

        public override IndicatorType ReadTypedValue(BinaryReader input, int dataSize) //throws IOException
        {
            return IndicatorType.Instance;
        }

        public override void WriteValue(BinaryWriter output, IndicatorType value) //throws IOException
        {
            // Intentional no-op.
        }
    }
}
