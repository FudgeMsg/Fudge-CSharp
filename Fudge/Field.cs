/**
* Copyright (C) 2009 - 2009 by OpenGamma Inc.
*
* Please see distribution for license.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGamma.Fudge
{
    /// <summary>
    /// <c>Field</c> is a convenience class to allow functional construction of messages.
    /// </summary>
    /// <example>
    /// The following example shows constructing a message containing two sub-messages:
    /// <code>
    /// var inputMsg = new FudgeMsg(   
    ///                     new Field("sub1",
    ///                         new Field("bibble", "fibble"),
    ///                         new Field(827, "Blibble")),
    ///                     new Field("sub2", 
    ///                         new Field("bibble9", 9837438),
    ///                         new Field(828, 82.77f)));
    /// </code>
    /// </example>
    public class Field : IFudgeField
    {
        private readonly object value;
        private readonly FudgeFieldType type;
        private readonly short? ordinal;
        private readonly string name;

        public Field(string name, object value) : this(name, null, value)
        {
        }

        public Field(int ordinal, object value) : this(null, ordinal, value)
        {
        }

        public Field(string name, params IFudgeField[] subFields) : this(name, null, new FudgeMsg(subFields))
        {
        }

        private Field(string name, int? ordinal, object value)
        {
            if (ordinal.HasValue && ordinal < short.MinValue || ordinal > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException("ordinal", "Ordinal must be within signed 16-bit range");
            }
            this.name = name;
            this.ordinal = (short?)ordinal;
            this.value = value;
            this.type = FudgeMsg.DetermineTypeFromValue(value);
        }

        #region IFudgeField Members

        public FudgeFieldType Type
        {
            get { return type; }
        }

        public object Value
        {
            get { return value; }
        }

        public short? Ordinal
        {
            get { return ordinal; }
        }

        public string Name
        {
            get { return name ; }
        }

        #endregion
    }
}
