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
using System.Text;
using Fudge.Taxon;
using System.IO;
using Fudge.Util;
using Fudge.Encodings;
using Fudge.Types;

namespace Fudge
{
    /// <summary>
    /// The primary entry-point for code to interact with the rest of the fudge system.
    /// For performance reasons, there are many options that are passed around as parameters
    /// inside static methods for encoding and decoding, and many lightweight objects that
    /// ideally don't know of their configuration context.
    /// However, in a large application, it is often desirable to collect all configuration
    /// parameters in one location and inject options into it.
    /// <p/>
    /// <c>FudgeContext</c> allows application developers to have a single location
    /// to inject dependent parameters and instances, and make them available through
    /// simple method invocations. in addition, because it wraps all checked exceptions
    /// into instances of <see cref="FudgeRuntimeException"/>, it is the ideal way to use
    /// the fudge encoding system from within spring applications.
    /// <p/>
    /// While most applications will have a single instance of <c>FudgeContext</c>,
    /// some applications will have one instance per unit of encoding/decoding parameters.
    /// for example, if an application is consuming data from two messaging feeds, each
    /// of which reuses the same taxonomy id to represent a different
    /// <see cref="IFudgeTaxonomy"/>, it would configure two different instances of
    /// <c>FudgeContext</c>, one per feed.  
    /// </summary>
    public class FudgeContext
    {
        private FudgeTypeDictionary typeDictionary = new FudgeTypeDictionary();
        private readonly FudgeTypeHandler typeHandler;
        private ITaxonomyResolver taxonomyResolver;
        private object[] properties;     // REVIEW 2009-11-28 t0rx -- Should we only create this on demand?
        private FudgeStreamParser parser;

        /// <summary>
        /// Constructs a new <see cref="FudgeContext"/>.
        /// </summary>
        public FudgeContext()
        {
            properties = new object[0];              // This will expand on use
            parser = new FudgeStreamParser(this);
            typeHandler = new FudgeTypeHandler(typeDictionary);
        }

        /// <summary>
        /// Gets or sets the <c>ITaxonomyResolver</c> for use within this context when encoding or decoding messages.
        /// </summary>
        public ITaxonomyResolver TaxonomyResolver
        {
            get { return taxonomyResolver; }
            set { taxonomyResolver = value; }
        }

        /// <summary>
        /// Gets or sets the <c>FudgeTypeDictionary</c> for use within this context when encoding or decoding messages.
        /// </summary>
        public FudgeTypeDictionary TypeDictionary
        {
            get { return typeDictionary; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", "Every Fudge context must have a type dictionary.");
                }
                typeDictionary = value;
                typeHandler.TypeDictionary = value;     // TODO 2009-12-23 t0rx -- This smells
            }
        }

        /// <summary>
        /// Gets the <see cref="FudgeTypeHandler"/> for this context.
        /// </summary>
        public FudgeTypeHandler TypeHandler
        {
            get { return typeHandler; }
        }

        /// <summary>
        /// Creates a new, empty <c>FudgeMsg</c> object.
        /// </summary>
        /// <returns>the <c>FudgeMsg</c> created</returns>
        public FudgeMsg NewMessage()
        {
            return new FudgeMsg(this);
        }

        /// <summary>
        /// Creates a new <see cref="FudgeMsg"/> containing the given fields.
        /// </summary>
        /// <param name="fields">Fields to add to message.</param>
        /// <returns>The new <see cref="FudgeMsg"/>.</returns>
        public FudgeMsg NewMessage(params IFudgeField[] fields)
        {
            return new FudgeMsg(this, fields);
        }

        /// <summary>
        /// Encodes a <c>FudgeMsg</c> object to a <c>Stream</c> without any taxonomy reference.
        /// </summary>
        /// <param name="msg">The message to serialise</param>
        /// <param name="s">The stream to serialise to</param>
        public void Serialize(FudgeMsg msg, Stream s)
        {
            Serialize(msg, null, s);
        }

        /// <summary>
        /// Encodes a <c>FudgeMsg</c> object to a <see cref="BinaryWriter"/> without any taxonomy reference.
        /// </summary>
        /// <param name="msg">The message to serialise</param>
        /// <param name="bw">The <see cref="BinaryWriter"/> to serialise to</param>
        public void Serialize(FudgeMsg msg, BinaryWriter bw)
        {
            Serialize(msg, null, bw);
        }

        /// <summary>
        /// Encodes a <c>FudgeMsg</c> object to a <c>Stream</c> with an optional taxonomy reference.
        /// If a taxonomy is supplied it may be used to optimize the output by writing ordinals instead
        /// of field names.
        /// </summary>
        /// <param name="msg">the <c>FudgeMessage</c> to write</param>
        /// <param name="taxonomyId">the identifier of the taxonomy to use. Specify <c>null</c> for no taxonomy</param>
        /// <param name="s">the <c>Stream</c> to write to</param>
        public void Serialize(FudgeMsg msg, short? taxonomyId, Stream s)
        {
            Serialize(msg, taxonomyId, new FudgeBinaryWriter(s));
        }

        /// <summary>
        /// Encodes a <c>FudgeMsg</c> object to a <c>Stream</c> with an optional taxonomy reference.
        /// If a taxonomy is supplied it may be used to optimize the output by writing ordinals instead
        /// of field names.
        /// </summary>
        /// <param name="msg">the <c>FudgeMessage</c> to write</param>
        /// <param name="taxonomyId">the identifier of the taxonomy to use. Specify <c>null</c> for no taxonomy</param>
        /// <param name="bw">The <see cref="BinaryWriter"/> to serialise to</param>
        public void Serialize(FudgeMsg msg, short? taxonomyId, BinaryWriter bw)
        {
            try
            {
                var writer = new FudgeEncodedStreamWriter(this);
                writer.TaxonomyId = taxonomyId;
                writer.Reset(bw);
                writer.StartMessage();
                writer.WriteFields(msg.GetAllFields());
                writer.EndMessage();
            }
            catch (IOException e)
            {
                throw new FudgeRuntimeException("Unable to write Fudge message to OutputStream", e);
            }
        }

        /// <summary>
        /// Returns the Fudge encoded form of a <c>FudgeMsg</c> as a <c>byte</c> array without a taxonomy reference.
        /// </summary>
        /// <param name="msg">the <c>FudgeMsg</c> to encode</param>
        /// <returns>an array containing the encoded message</returns>
        public byte[] ToByteArray(FudgeMsg msg)
        {
            MemoryStream stream = new MemoryStream();
            Serialize(msg, stream);
            return stream.ToArray();
        }

        // TODO 2009-12-11 Andrew -- should we have a toByteArray that takes a taxonomy too?

        // TODO 2009-12-11 Andrew -- should we have a Serialize that takes the envelope as there's no way to control the version field otherwise?

        /// <summary>
        /// Decodes a <c>FudgeMsg</c> from a <c>Stream</c>.
        /// </summary>
        /// <param name="s">the <c>Stream</c> to read encoded data from</param>
        /// <returns>the next <c>FudgeMsgEnvelope</c> encoded on the stream</returns>
        public FudgeMsgEnvelope Deserialize(Stream s)
        {
            FudgeMsgEnvelope envelope;
            try
            {
                // TODO 2009-12-23 t0rx -- Should this now be refactored to use FudgeMsgStreamReader?
                envelope = parser.Parse(s);
            }
            catch (IOException e)
            {
                throw new FudgeRuntimeException("Unable to deserialize FudgeMsg from input stream", e);
            }
            return envelope;
        }

        /// <summary>
        /// Decodes a <c>FudgeMsg</c> from a <c>byte</c> array. If the array is larger than the Fudge envelope, any additional data is ignored.
        /// </summary>
        /// <param name="bytes">an array containing the envelope encoded <c>FudgeMsg</c></param>
        /// <returns>the decoded <c>FudgeMsgEnvelope</c></returns>
        public FudgeMsgEnvelope Deserialize(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes, false);
            return Deserialize(stream);
        }

        // TODO 2009-12-11 Andrew -- should we have a version that takes an offset so that arrays with more than one envelope can be processed?
        //      2009-12-23 t0rx -- or is that actually about the FudgeEncodedStreamReader?

        #region Property support

        /// <summary>
        /// Gets the value of a specific property from this context, or null if not set.
        /// </summary>
        /// <param name="prop">Property to retrieve.</param>
        /// <returns>Property value or null if not set.</returns>
        public object GetProperty(FudgeContextProperty prop)
        {
            if (prop == null)
                throw new ArgumentNullException("prop");

            int index = prop.Index;
            if (index >= properties.Length)
                return null;

            return properties[index];
        }

        /// <summary>
        /// Gets the value of a specific property from this context, or returns <c>defaultValue</c> if not set.
        /// </summary>
        /// <param name="prop">Property to retrieve.</param>
        /// <param name="defaultValue">Value to return if property not set.</param>
        /// <returns>Property value or <c>defaultValue</c> if not set.</returns>
        public object GetProperty(FudgeContextProperty prop, object defaultValue)
        {
            return GetProperty(prop) ?? defaultValue;
        }

        /// <summary>
        /// Sets the value of a specific property in the context.
        /// </summary>
        /// <param name="prop">Property to set.</param>
        /// <param name="value">Value for the property.</param>
        /// <remarks>
        /// Context properties are used to control the behaviour of encoding and decoding.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is rejected by the <see cref="FudgeContextProperty"/> as invalid.</exception>
        public void SetProperty(FudgeContextProperty prop, object value)
        {
            if (prop == null)
                throw new ArgumentNullException("prop");
            if (!prop.IsValidValue(value))
                throw new ArgumentOutOfRangeException("Value is not valid for context property " + prop.Name);

            int index = prop.Index;
            if (index >= properties.Length)
            {
                lock (this)
                {
                    if (index >= properties.Length)
                    {
                        int newSize = Math.Max(properties.Length, FudgeContextProperty.MaxIndex + 1);
                        var newArray = new object[newSize];
                        properties.CopyTo(newArray, 0);
                        properties = newArray;
                    }
                }
            }

            properties[index] = value;
        }

        #endregion

        /// <summary>
        /// <c>FudgeTypeHandler</c> provides methods to handle type-related functions.
        /// </summary>
        public class FudgeTypeHandler
        {
            private FudgeTypeDictionary typeDictionary;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="typeDictionary"></param>
            public FudgeTypeHandler(FudgeTypeDictionary typeDictionary)
            {
                this.typeDictionary = typeDictionary;
            }

            internal FudgeTypeDictionary TypeDictionary
            {
                set { typeDictionary = value; }
            }

            /// <summary>
            /// Converts the supplied value to a base type using the corresponding FudgeFieldType definition. The supplied .NET type
            /// is resolved to a registered FudgeFieldType. The <c>ConvertValueFrom</c> method on the registered type is then used
            /// to convert the value.
            /// </summary>
            /// <param name="value">value to convert</param>
            /// <param name="type">.NET target type</param>
            /// <returns>the converted value</returns>
            public object ConvertType(object value, Type type)
            {
                if (value == null) return null;
                if (value.GetType() == type) return value;

                if (!type.IsAssignableFrom(value.GetType()))
                {
                    FudgeFieldType fieldType = typeDictionary.GetByCSharpType(type);
                    if (fieldType == null)
                        throw new InvalidCastException("No registered field type for " + type.Name);

                    value = fieldType.ConvertValueFrom(value);
                }
                return value;
            }

            /// <summary>
            /// Determines the <c>FudgeFieldType</c> of a C# value.
            /// </summary>
            /// <param name="value">value whose type is to be determined</param>
            /// <returns>the appropriate <c>FudgeFieldType</c> instance</returns>
            public FudgeFieldType DetermineTypeFromValue(object value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Cannot determine type for null value.");
                }
                FudgeFieldType type = typeDictionary.GetByCSharpType(value.GetType());
                if (type == null)
                {
                    // Couple of special cases
                    if (value is UnknownFudgeFieldValue)
                    {
                        UnknownFudgeFieldValue unknownValue = (UnknownFudgeFieldValue)value;
                        type = unknownValue.Type;
                    }
                    else if (value is IFudgeFieldContainer)
                    {
                        type = FudgeMsgFieldType.Instance;
                    }
                }
                return type;
            }
        }
    }
}
