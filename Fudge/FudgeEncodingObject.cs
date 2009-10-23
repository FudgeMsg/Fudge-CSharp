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
using Fudge.Taxon;
using System.Diagnostics;

namespace Fudge
{
    /// <summary>
    /// The base type for all objects which can be encoded using Fudge. 
    /// </summary>
    public abstract class FudgeEncodingObject
    {
        private Dictionary<IFudgeTaxonomy, int> sizesByTaxonomy;
        private volatile int noTaxonomySize = -1;

        public int GetSize(IFudgeTaxonomy taxonomy)
        {
            if (taxonomy == null)
            {
                if (noTaxonomySize == -1)
                {
                    noTaxonomySize = ComputeSize(null);
                }
                return noTaxonomySize;
            }
            lock (this)     // TODO t0rx 2009-09-01 -- Should this lock something internal in case someone else has us locked?
            {
                if (sizesByTaxonomy == null)
                {
                    sizesByTaxonomy = new Dictionary<IFudgeTaxonomy, int>();
                }
                int result;
                if (!sizesByTaxonomy.TryGetValue(taxonomy, out result))
                {
                    result = ComputeSize(taxonomy);
                    sizesByTaxonomy.Add(taxonomy, result);
                }
                return result;
            }
        }

        public abstract int ComputeSize(IFudgeTaxonomy taxonomy);
    }
}
