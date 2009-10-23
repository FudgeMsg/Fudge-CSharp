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

namespace Fudge.Taxon
{
    /// <summary>
    /// An implementation of <see cref="TaxonomyResolver"/> which is backed by a <see cref="Dictionary"/>.
    /// This is mostly useful where the entire set of taxonomies is known at module
    /// initialization (or compilation) time. As for performance reasons the
    /// <see cref="Dictionary"/> is fixed at instantiation time, it is not appropriate for
    /// situations where the set of taxonomies will change at runtime.
    /// </summary>
    public class ImmutableMapTaxonomyResolver : ITaxonomyResolver
    {
        private static readonly Dictionary<int, IFudgeTaxonomy> emptyDictionary = new Dictionary<int, IFudgeTaxonomy>();
        private readonly Dictionary<int, IFudgeTaxonomy> taxonomiesById;

        /// <summary>
        /// The default constructor will result in a resolver that never
        /// resolves any taxonomies.
        /// </summary>
        public ImmutableMapTaxonomyResolver()
            : this(emptyDictionary)
        {
        }

        public ImmutableMapTaxonomyResolver(Dictionary<int, IFudgeTaxonomy> taxonomiesById)
        {
            if (taxonomiesById == null)
            {
                taxonomiesById = emptyDictionary;
            }
            this.taxonomiesById = new Dictionary<int, IFudgeTaxonomy>(taxonomiesById);
        }

        #region ITaxonomyResolver Members

        public IFudgeTaxonomy ResolveTaxonomy(short taxonomyId)
        {
            IFudgeTaxonomy result;
            return taxonomiesById.TryGetValue(taxonomyId, out result) ? result : null;
        }

        #endregion
    }
}
