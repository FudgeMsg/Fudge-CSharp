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
using Fudge.Serialization.Reflection;
using System.Runtime.Serialization;

namespace Fudge.Serialization
{
    /// <summary>
    /// The <c>Fudge.Serialization</c> namespace contains classes and interfaces to enable serialization
    /// to a Fudge message stream and deserialization again from it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="FudgeSerializer"/> can serialize to any kind stream supported by an <see cref="IFudgeStreamReader"/> and
    /// <see cref="IFudgeStreamWriter"/>, such as Fudge binary encoding, XML, or a sequence of
    /// <see cref="FudgeMsg"/> objects.  See the <see cref="Fudge.Encodings"/> namespace for the
    /// full list of supported formats.
    /// </para>
    /// <para>
    /// Serialization of a class can be done in a number of ways:
    /// <list type="table">
    /// <item>
    /// <term>Bean-style</term>
    /// <description>If the class has properties with <c>get</c> and <c>set</c> for each, then the Fudge serialization framework will automatically
    /// generate a <see cref="PropertyBasedSerializationSurrogate"/> to handle it.</description>
    /// </item>
    /// <item>
    /// <term>Immutable</term>
    /// <description>If the class has public getters but no setters, and a constructor with arguments that match the properties (ignoring case), then
    /// a <see cref="ImmutableSurrogate"/> will automatically be generated.</description>
    /// </item>
    /// <item>
    /// <term>Direct</term>
    /// <description>The class can implement <see cref="IFudgeSerializable"/> and handle its own serialization and deserialization.  This requires it to have
    /// a default constructor.</description>
    /// </item>
    /// <item>
    /// <term>
    /// Surrogate
    /// </term>
    /// <description>Serialization can be implemented in a separate surrogate class which implements <see cref="IFudgeSerializationSurrogate"/> to perform the
    /// serialization and deserialization.  This can  be specified either through the <see cref="FudgeSurrogateAttribute"/> attribute on the class, or by
    /// explicitly registering the surrogate with the <see cref="FudgeSerializer.TypeMap"/>.</description>
    /// </item>
    /// <item>
    /// <term>Convention</term>
    /// <description>If the class implements <c>public void ToFudgeMsg(IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)</c> and
    /// <c>public static &lt;YourType&gt; FromFudgeMsg(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)</c> then these will be used
    /// to serialize and deserialize.</description>
    /// </item>
    /// <item>
    /// <term>ISerializable</term>
    /// <description>Classes that have been written for the original .net serialization framework and use <see cref="ISerializable"/> to perform custom
    /// serialization may be used directly.  This is lower performance than using <see cref="IFudgeSerializable"/> because data must be marshalled via
    /// a <see cref="SerializationInfo"/>.  Also note the issues with <c>null</c> described in <see cref="Reflection.DotNetSerializableSurrogate"/>.</description>
    /// </item>
    /// <item>
    /// <term>ISerializationSurrogate</term>
    /// <description>Classes written for the original .net serialization framework and using an <see cref="ISerializationSurrogate"/> are also supported with
    /// the same notes as for ISerializable above.  The <see cref="SurrogateSelector"/> is registered in the <see cref="SerializationTypeMap"/> or through
    /// the <see cref="ContextProperties.DotNetSurrogateSelectorProperty"/> context property.</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// To support the different naming conventions of other languages that you may be interoperating with, Fudge serialization allows you
    /// to modify the conventions for naming types and naming fields through the <see cref="ContextProperties.TypeMappingStrategyProperty"/>
    /// and <see cref="ContextProperties.FieldNameConventionProperty"/> context properties.  These would allow you for example to match
    /// Java conventions by converting <c>Fudge.Serialization.FudgeSerializer</c> to <c>org.fudgemsg.serialization.FudgeSerializer</c>
    /// and make fields by default <c>camelCase</c> rather than <c>PascalCase</c> - this is implemented by the
    /// <see cref="JavaTypeMappingStrategy"/> class.
    /// </para>
    /// <para>
    /// When using runtime-generated surrogates, serialization of fields can further be controlled through the use of the
    /// <see cref="FudgeTransientAttribute"/> and <see cref="FudgeFieldNameAttribute"/> attributes.
    /// </para>
    /// </remarks>
    /// <example>
    /// The following example shows how to serialize an object to a Fudge-encoded binary stream:
    /// <code>
    /// // Create a context and a serializer
    /// var context = new FudgeContext();
    /// var serializer = new FudgeSerializer(context);
    /// 
    /// // Our object to serialize
    /// var temperatureRange = new TemperatureRange { High = 28.3, Low = 13.2, Average = 19.6 };
    /// 
    /// // Serialize it to a MemoryStream
    /// var stream = new MemoryStream();
    /// var streamWriter = new FudgeEncodedStreamWriter(context, stream);
    /// serializer.Serialize(streamWriter, temperatureRange);
    /// 
    /// // Reset the stream and deserialize a new object from it
    /// stream.Position = 0;
    /// var streamReader = new FudgeEncodedStreamReader(context, stream);
    /// var range2 = (TemperatureRange)serializer.Deserialize(streamReader);
    /// 
    /// // Just check a value matches
    /// Debug.Assert(range2.Average == 19.6);
    /// </code>
    /// </example>
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}
