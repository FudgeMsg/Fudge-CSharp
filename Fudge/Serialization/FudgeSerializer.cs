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
using Fudge.Encodings;
using Fudge.Types;
using Fudge.Serialization.Reflection;

namespace Fudge.Serialization
{
    /// <summary>
    /// The main entry-point for performing serialization and deserialization of .net objects with Fudge.
    /// </summary>
    /// <remarks>
    /// For exmaples and more information on the serialization capabilities of the Fudge serialization framework,
    /// please see the <see cref="Fudge.Serialization"/> namespace documentation.
    /// </remarks>
    public class FudgeSerializer
    {
        private readonly FudgeContext context;
        private readonly SerializationTypeMap typeMap;

        /// <summary>Constant defining the ordinal for the field in which type information is stored.</summary>
        public const int TypeIdFieldOrdinal = 0;

        /// <summary>Property of the <see cref="FudgeContext"/> that overrides the default value of <see cref="TypeMappingStrategy"/>.</summary>
        public static readonly FudgeContextProperty TypeMappingStrategyProperty = new FudgeContextProperty("Serialization.TypeMappingStrategy", typeof(IFudgeTypeMappingStrategy));
        /// <summary>Property of the <see cref="FudgeContext"/> that sets the <see cref="FudgeFieldNameConvention"/> (<see cref="FudgeFieldNameConvention.Identity"/> by default).</summary>
        public static readonly FudgeContextProperty FieldNameConventionProperty = new FudgeContextProperty("Serialization.FieldNameConvention", typeof(FudgeFieldNameConvention));
        /// <summary>Property of the <see cref="FudgeContext"/> that specifies whether types can automatically by serialized or whether they must be explicitly
        /// registered in the <see cref="SerializationTypeMap"/>.  By default this is <c>true</c>, i.e. types do not need explicitly registering.</summary>
        public static readonly FudgeContextProperty AllowTypeDiscoveryProperty = new FudgeContextProperty("Serialization.AllowTypeDiscovery", typeof(bool));
        /// <summary>Property of the <see cref="FudgeContext"/> that specifies whether types are by default inlined rather than referenced.  If
        /// you leave this unspecified then if will default to <c>false</c> - i.e. objects are referenced.</summary>
        /// <remarks>Whilst inlining sub-objects makes the message structure more readable, it prohibits cycles in the object graph and also means that
        /// a sub-object that appears more than once in the graph will be serialized multiple times and deserialized as multiple objects.</remarks>
        public static readonly FudgeContextProperty InlineByDefault = new FudgeContextProperty("Serialization.InlineByDefault", typeof(bool));

        /// <summary>
        /// Constructs a new <see cref="FudgeSerializer"/> instance.
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> for the serializer.</param>
        public FudgeSerializer(FudgeContext context)
            : this(context, null)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="FudgeSerializer"/> instance.
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> for the serializer.</param>
        /// <param name="typeMap">Typemap to use rather than creating a default one.</param>
        public FudgeSerializer(FudgeContext context, SerializationTypeMap typeMap)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (typeMap == null)
            {
                // TODO 2010-02-02 t0rx -- Have serialization type map as context property?
                typeMap = new SerializationTypeMap(context);
            }

            this.context = context;
            this.typeMap = typeMap;

            this.TypeMappingStrategy = (IFudgeTypeMappingStrategy)context.GetProperty(TypeMappingStrategyProperty, new DefaultTypeMappingStrategy());
        }

        /// <summary>
        /// Gets and sets the strategy to use for mapping .net types to and from
        /// </summary>
        public IFudgeTypeMappingStrategy TypeMappingStrategy { get; set; }

        /// <summary>
        /// Gets the <see cref="SerializationTypeMap"/> used by this serializer.
        /// </summary>
        public SerializationTypeMap TypeMap
        {
            get { return typeMap; }
        }

        /// <summary>
        /// Serializes an object graph to a Fudge message stream.
        /// </summary>
        /// <param name="writer">Stream to write the messages to.</param>
        /// <param name="graph">Starting point for graph of objects to serialize.</param>
        public void Serialize(IFudgeStreamWriter writer, object graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException("graph");
            }

            // Delegate to FudgeSerializationContext to do the work
            var serializationContext = new FudgeSerializationContext(context, typeMap, writer, TypeMappingStrategy);
            serializationContext.SerializeGraph(writer, graph);
        }

        /// <summary>
        /// Convenience method to serializae an object graph to a list of <see cref="FudgeMsg"/> objects.
        /// </summary>
        /// <param name="graph">Starting point for graph of objects to serialize.</param>
        /// <returns>List of FudgeMsg objects containing the serialized state.</returns>
        public IList<FudgeMsg> SerializeToMsgs(object graph)
        {
            var writer = new FudgeMsgStreamWriter(context);
            Serialize(writer, graph);
            return writer.PeekAllMessages();
        }

        /// <summary>
        /// Deserializes an object graph from a message stream.
        /// </summary>
        /// <param name="reader">Reader to get messages from the underlying stream.</param>
        /// <returns>Deserialized object graph.</returns>
        public object Deserialize(IFudgeStreamReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Delegate to FudgeDeserializer to do the work
            var deserializer = new FudgeDeserializationContext(context, typeMap, reader, TypeMappingStrategy);
            return deserializer.DeserializeGraph();
        }

        /// <summary>
        /// Convenience method to deserialize an object graph from a list of messages.
        /// </summary>
        /// <param name="msgs">Messages containing serialized state.</param>
        /// <returns>Deserialized object graph.</returns>
        public object Deserialize(IEnumerable<FudgeMsg> msgs)
        {
            var reader = new FudgeMsgStreamReader(context, msgs);
            return Deserialize(reader);
        }
    }
}
