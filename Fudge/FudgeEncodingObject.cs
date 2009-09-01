/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge.Taxon;
using System.Diagnostics;

namespace OpenGamma.Fudge
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
            lock (this)     // TODO: 20090901 (t0rx): Should this lock something internal in case someone else has us locked?
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
