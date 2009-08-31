/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGamma.Fudge.Taxon
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
        private static readonly Dictionary<short, IFudgeTaxonomy> emptyDictionary = new Dictionary<short, IFudgeTaxonomy>();
        private readonly Dictionary<short, IFudgeTaxonomy> taxonomiesById;

        /// <summary>
        /// The default constructor will result in a resolver that never
        /// resolves any taxonomies.
        /// </summary>
        public ImmutableMapTaxonomyResolver()
            : this(emptyDictionary)
        {
        }

        public ImmutableMapTaxonomyResolver(Dictionary<short, IFudgeTaxonomy> taxonomiesById)
        {
            if (taxonomiesById == null)
            {
                taxonomiesById = emptyDictionary;
            }
            this.taxonomiesById = new Dictionary<short, IFudgeTaxonomy>(taxonomiesById);
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
