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
    /// A Taxonomy Resolver can identify a <see cref="IFudgeTaxonomy"/> instance that is
    /// appropriate for a message with a specific taxonomy ID.
    /// This ID is actually appropriate for a particular application, and possibly
    /// for a particular point in time. In fact, it may be appropriate for a particular
    /// stream of data from a particular source, and some applications may have
    /// multiple <c>ITaxonomyResolver</c>s loaded into a single application. 
    /// </summary>
    /// <seealso cref="TaxonomyResolver"/>
    public interface ITaxonomyResolver
    {
        /// <summary>
        /// Identify the taxonomy that should be used to resolve names with the
        /// specified ID.
        /// </summary>
        /// <param name="taxonomyId">The ID of the taxonomy to load</param>
        /// <returns>The taxonomy, or <c>null</c></returns>
        IFudgeTaxonomy ResolveTaxonomy(short taxonomyId);
    }

    /// <summary>
    /// Delegate form for Taxonomy Resolver for when you don't need to create an object
    /// </summary>
    /// <seealso cref="ITaxonomyResolver"/>
    /// <param name="taxonomyId">The ID of the taxonomy to load</param>
    /// <returns>The taxonomy, or <c>null</c></returns>
    public delegate IFudgeTaxonomy TaxonomyResolver(short taxonomyId);
}
