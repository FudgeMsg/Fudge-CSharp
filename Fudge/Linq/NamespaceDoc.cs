/* <!--
 * Copyright (C) 2009 - 2010 by OpenGamma Inc. and other contributors.
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
using System.Runtime.CompilerServices;

namespace Fudge.Linq
{
    /// <summary>
    /// The <see cref="Fudge.Linq"/> namespace makes lists of <see cref="FudgeMsg"/> objects
    /// available for processing in Linq expressions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In the same way as Linq-to-SQL, Linq-to-Fudge relies on there being a proper type that the message
    /// can be mapped onto (i.e. a property with the same name and type as each field that we need to operate
    /// on in the message).  This is only used to make Intellisense and the compiler work - no actual
    /// deserialisation into objects happens whilst processing.
    /// </para>
    /// <para>
    /// For more information on using Linq, see the <see cref="System.Linq"/> namespace documentation.
    /// </para>
    /// <para>
    /// Linq-to-Fudge is built upon the IQToolkit, which is available at http://www.codeplex.com/IQToolkit
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows how you use Fudge-to-Linq to be able to query a list of <see cref="FudgeMsg"/> objects:
    /// <code>
    /// class Tick
    /// {
    ///     // Model the data we are going to process
    ///     public double Bid { get; set; }
    ///     public double Ask { get; set; }
    ///     public string Ticker { get; set; }
    /// }
    /// 
    /// FudgeMsg CreateTickMsg(double bid, double ask, string ticker)
    /// {
    ///     FudgeMsg msg = new FudgeMsg(
    ///                         new Field("Bid", bid),
    ///                         new Field("Ask", ask),
    ///                         new Field("Ticker", ticker));
    ///     return msg;
    /// }
    /// 
    /// // ...
    /// 
    /// // Create some example messages
    /// var msgs = new FudgeMsg[] { CreateTickMsg(10.3, 11.1, "FOO"), CreateTickMsg(2.4, 3.1, "BAR") };
    /// 
    /// // Query them in Linq
    /// var query = from tick in msgs.AsQueryable&lt;Tick&gt;()
    ///             where tick.Ask > 4.0
    ///             select new { tick.Ticker, tick.Ask };
    /// 
    /// // Get the results as an array so we can work with them
    /// var results = query.ToArray();
    /// </code>
    /// </example>
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}
