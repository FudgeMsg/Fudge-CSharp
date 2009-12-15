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

namespace Fudge.Taxon
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
