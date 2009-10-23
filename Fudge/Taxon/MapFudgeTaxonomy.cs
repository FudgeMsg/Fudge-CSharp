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
    /// An implementation of <see cref="IFudgeTaxonomy"/> where all lookups are specified
    /// at construction time and held in a <see cref="Dictionary"/>.
    /// This is extremely useful in a case where the taxonomy is generated dynamically,
    /// or as a building block for loading taxonomy definitions from persistent
    /// storage.

    /// </summary>
    public class MapFudgeTaxonomy : IFudgeTaxonomy
    {
        private readonly Dictionary<int, string> namesByOrdinal;
        private readonly Dictionary<string, int> ordinalsByName;
        private static readonly Dictionary<int, string> emptyDictionary = new Dictionary<int, string>();

        public MapFudgeTaxonomy() :
            this(emptyDictionary)
        {
        }

        public MapFudgeTaxonomy(Dictionary<int, string> namesByOrdinal)
        {
            if (namesByOrdinal == null)
            {
                namesByOrdinal = emptyDictionary;
            }
            this.namesByOrdinal = new Dictionary<int, string>(namesByOrdinal);
            this.ordinalsByName = new Dictionary<string, int>(namesByOrdinal.Count);
            foreach (var entry in namesByOrdinal)
            {
                ordinalsByName.Add(entry.Value, entry.Key);
            }
        }

        public MapFudgeTaxonomy(int[] ordinals, string[] names)
        {
            if (ordinals == null)
            {
                throw new ArgumentNullException("Must provide ordinals.");
            }
            if (names == null)
            {
                throw new ArgumentNullException("Must provide names.");
            }
            if (ordinals.Length != names.Length)
            {
                throw new ArgumentException("Arrays of ordinals and names must be of same length.");
            }
            namesByOrdinal = new Dictionary<int, string>(ordinals.Length);
            ordinalsByName = new Dictionary<string, int>(ordinals.Length);
            for (int i = 0; i < ordinals.Length; i++)
            {
                namesByOrdinal.Add(ordinals[i], names[i]);
                ordinalsByName.Add(names[i], ordinals[i]);
            }
        }

        #region IFudgeTaxonomy Members

        public string GetFieldName(short ordinal)
        {
            string result;
            return namesByOrdinal.TryGetValue(ordinal, out result) ? result : null;
        }

        public short? GetFieldOrdinal(string fieldName)
        {
            int result;
            return ordinalsByName.TryGetValue(fieldName, out result) ? (short?)result : null;
        }

        #endregion
    }
}
