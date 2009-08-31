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
    /// An implementation of <see cref="IFudgeTaxonomy"/> where all lookups are specified
    /// at construction time and held in a <see cref="Dictionary"/>.
    /// This is extremely useful in a case where the taxonomy is generated dynamically,
    /// or as a building block for loading taxonomy definitions from persistent
    /// storage.

    /// </summary>
    public class MapFudgeTaxonomy : IFudgeTaxonomy
    {
        private readonly Dictionary<short, string> namesByOrdinal;
        private readonly Dictionary<string, short> ordinalsByName;
        private static readonly Dictionary<short, string> emptyDictionary = new Dictionary<short, string>();

        public MapFudgeTaxonomy() :
            this(emptyDictionary)
        {
        }

        public MapFudgeTaxonomy(Dictionary<short, string> namesByOrdinal)
        {
            if (namesByOrdinal == null)
            {
                namesByOrdinal = emptyDictionary;
            }
            this.namesByOrdinal = new Dictionary<short, string>(namesByOrdinal);
            this.ordinalsByName = new Dictionary<string, short>(namesByOrdinal.Count);
            foreach (var entry in namesByOrdinal)
            {
                ordinalsByName.Add(entry.Value, entry.Key);
            }
        }

        public MapFudgeTaxonomy(short[] ordinals, string[] names)
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
            namesByOrdinal = new Dictionary<short, string>(ordinals.Length);
            ordinalsByName = new Dictionary<string, short>(ordinals.Length);
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
            short result;
            return ordinalsByName.TryGetValue(fieldName, out result) ? (short?)result : null;
        }

        #endregion
    }
}
