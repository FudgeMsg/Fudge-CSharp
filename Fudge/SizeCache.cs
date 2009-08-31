/**
* Copyright (C) 2009 - 2009 by OpenGamma Inc.
*
* Please see distribution for license.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using OpenGamma.Fudge.Taxon;

namespace OpenGamma.Fudge
{
    /// <summary>
    /// Stores the sizes maintained by <see cref="FudgeMsg"/> and <see cref="FudgeMsgField"/>
    /// keyed by whether there's a taxonomy provided or not.
    /// </summary>
    internal class SizeCache
    {
        private readonly Dictionary<IFudgeTaxonomy, int> sizesByTaxonomy = new Dictionary<IFudgeTaxonomy, int>();       // TODO: 20090830 (t0rx): In Fudge-Java this is ConcurrentHashMap
        private volatile int noTaxonomySize = -1;              // TODO: 20090830 (t0rx): Check volatility has correct behaviour in C#
        private readonly ISizeComputable sizeComputable;

        public SizeCache(ISizeComputable sizeComputable)
        {
            Debug.Assert(sizeComputable != null);
            this.sizeComputable = sizeComputable;
        }

        public int GetSize(IFudgeTaxonomy taxonomy)
        {
            if (taxonomy == null)
            {
                if (noTaxonomySize == -1)
                {
                    noTaxonomySize = sizeComputable.ComputeSize(null);
                }
                return noTaxonomySize;
            }
            int result = -1;
            if (!sizesByTaxonomy.TryGetValue(taxonomy, out result))
            {
                result = sizeComputable.ComputeSize(taxonomy);
                sizesByTaxonomy.Add(taxonomy, result);
            }
            Debug.Assert(result >= 0);
            return result;
        }
    }
}
