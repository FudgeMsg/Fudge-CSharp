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
                // REVIEW 2010-02-02 t0rx -- Have serialization type map as context property?
                typeMap = new SerializationTypeMap(context);
            }

            this.context = context;
            this.typeMap = typeMap;

            this.TypeMappingStrategy = (IFudgeTypeMappingStrategy)context.GetProperty(ContextProperties.TypeMappingStrategyProperty, new DefaultTypeMappingStrategy());
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
            serializationContext.SerializeGraph(graph);
        }

        /// <summary>
        /// Convenience method to serializae an object graph to a list of <see cref="FudgeMsg"/> objects.
        /// </summary>
        /// <param name="graph">Starting point for graph of objects to serialize.</param>
        /// <returns>List of FudgeMsg objects containing the serialized state.</returns>
        public FudgeMsg SerializeToMsg(object graph)
        {
            var writer = new FudgeMsgStreamWriter(context);
            Serialize(writer, graph);
            return writer.DequeueMessage();
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
        /// <param name="msg">Message containing serialized state.</param>
        /// <returns>Deserialized object graph.</returns>
        public object Deserialize(FudgeMsg msg)
        {
            var reader = new FudgeMsgStreamReader(context, new FudgeMsg[] {msg});
            return Deserialize(reader);
        }
    }
}
