/**
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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge.Taxon;
using System.IO;

namespace OpenGamma.Fudge.Types
{
    public abstract class FudgeArrayFieldTypeBase<T> : FudgeFieldType<T[]>
    {
        private readonly int elementSize;
        private readonly Action<BinaryWriter, T> writer;
        private readonly Func<BinaryReader, T> reader;

        public FudgeArrayFieldTypeBase(int typeId, int elementSize, Action<BinaryWriter, T> writer, Func<BinaryReader, T> reader)
            : base(typeId, true, 0)
        {
            this.elementSize = elementSize;
            this.writer = writer;
            this.reader = reader;
        }

        public override int GetVariableSize(T[] value, IFudgeTaxonomy taxonomy)
        {
            return value.Length * elementSize;
        }

        public override T[] ReadTypedValue(BinaryReader input, int dataSize, FudgeTypeDictionary typeDictionary) //throws IOException
        {
            int nElements = dataSize / elementSize;
            T[] result = new T[nElements];
            for (int i = 0; i < nElements; i++)
            {
                result[i] = reader(input);
            }
            return result;
        }

        public override void WriteValue(BinaryWriter output, T[] value, IFudgeTaxonomy taxonomy)  //throws IOException
        {
            foreach (T element in value)
            {
                writer(output, element);
            }
        }
    }
}
