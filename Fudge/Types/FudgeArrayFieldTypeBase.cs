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
    /// Base class for arrays of the primitive types predefined within Fudge. This contains all of the common functionality shared by the
    /// array type implementations.
    /// </summary>
    /// <typeparam name="T">base .NET type</typeparam>
    public abstract class FudgeArrayFieldTypeBase<T> : FudgeFieldType<T[]>
    {
        private readonly int elementSize;
        private readonly Action<BinaryWriter, T> writer;
        private readonly Func<BinaryReader, T> reader;

        /// <summary>
        /// Creates a new base type.
        /// </summary>
        /// <param name="typeId">Fudge type ID</param>
        /// <param name="elementSize">encoded size of an array element (in bytes)</param>
        /// <param name="writer">delegate for writing an array element</param>
        /// <param name="reader">delegate for reading an array element</param>
        public FudgeArrayFieldTypeBase(int typeId, int elementSize, Action<BinaryWriter, T> writer, Func<BinaryReader, T> reader)
            : base(typeId, true, 0)
        {
            this.elementSize = elementSize;
            this.writer = writer;
            this.reader = reader;
        }

        /// <inheritdoc cref="Fudge.FudgeFieldType.GetVariableSize(System.Object,Fudge.Taxon.IFudgeTaxonomy)" />
        public override int GetVariableSize(T[] value, IFudgeTaxonomy taxonomy)
        {
            return value.Length * elementSize;
        }

        /// <inheritdoc />
        public override T[] ReadTypedValue(BinaryReader input, int dataSize) //throws IOException
        {
            int nElements = dataSize / elementSize;
            T[] result = new T[nElements];
            for (int i = 0; i < nElements; i++)
            {
                result[i] = reader(input);
            }
            return result;
        }

        /// <inheritdoc />
        public override void WriteValue(BinaryWriter output, T[] value)  //throws IOException
        {
            foreach (T element in value)
            {
                writer(output, element);
            }
        }
    }
}
