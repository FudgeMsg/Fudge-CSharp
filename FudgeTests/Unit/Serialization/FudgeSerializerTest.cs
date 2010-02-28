/**
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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Fudge.Serialization;
using System.IO;
using Fudge.Encodings;
using System.Diagnostics;
using Fudge.Types;

namespace Fudge.Tests.Unit.Serialization
{
    public class FudgeSerializerTest
    {
        private readonly FudgeContext context = new FudgeContext();

        [Fact]
        public void VerySimpleExample()
        {
            // This code is used as the example in the NamespaceDoc for Fudge.Serialization

            // Create a context and a serializer
            var context = new FudgeContext();
            var serializer = new FudgeSerializer(context);

            // Our object to serialize
            var temperatureRange = new TemperatureRange { High = 28.3, Low = 13.2, Average = 19.6 };

            // Serialize it to a MemoryStream
            var stream = new MemoryStream();
            var streamWriter = new FudgeEncodedStreamWriter(context, stream);
            serializer.Serialize(streamWriter, temperatureRange);

            // Reset the stream and deserialize a new object from it
            stream.Position = 0;
            var streamReader = new FudgeEncodedStreamReader(context, stream);
            var range2 = (TemperatureRange)serializer.Deserialize(streamReader);

            // Just check a value matches
            Debug.Assert(range2.Average == 19.6);
        }


        [Fact]
        public void SimpleExampleWithSurrogate()
        {
            var typeMap = new SerializationTypeMap(context);
            typeMap.RegisterType(typeof(Explicit.Address), "Address", c => new Explicit.AddressSerializer());
            var serializer = new FudgeSerializer(context, typeMap);

            var address = new Explicit.Address("Our House", "In the middle of our street", "MD1");
            var msgs = serializer.SerializeToMsgs(address);

            var address2 = (Explicit.Address)serializer.Deserialize(msgs);

            Assert.Equal(address.Line1, address2.Line1);
            Assert.Equal(address.Line2, address2.Line2);
            Assert.Equal(address.Zip, address2.Zip);
        }

        [Fact]
        public void SimpleExampleWithIFudgeSerializable()
        {
            var typeMap = new SerializationTypeMap(context);
            typeMap.RegisterType(typeof(Explicit.Tick), "Tick");
            var serializer = new FudgeSerializer(context, typeMap);

            var tick = new Explicit.Tick { Ticker = "FOO", Bid = 12.3, Offer = 12.9 };
            var msgs = serializer.SerializeToMsgs(tick);

            var tick2 = (Explicit.Tick)serializer.Deserialize(msgs);

            Assert.Equal(tick.Ticker, tick2.Ticker);
            Assert.Equal(tick.Bid, tick2.Bid);
            Assert.Equal(tick.Offer, tick2.Offer);
        }        

        [Fact]
        public void InlineObject()
        {
            var typeMap = new SerializationTypeMap(context);
            typeMap.RegisterType(typeof(Explicit.Person), "Person");
            typeMap.RegisterType(typeof(Explicit.Address), "Address", new Explicit.AddressSerializer());
            var serializer = new FudgeSerializer(context, typeMap);

            var person = new Explicit.Person { Name = "Bob", MainAddress = new Explicit.Address("Foo", "Bar", null) };
            var msgs = serializer.SerializeToMsgs(person);

            var person2 = (Explicit.Person)serializer.Deserialize(msgs);
            Assert.NotSame(person.MainAddress, person2.MainAddress);
            Assert.Equal(person.MainAddress.Line1, person2.MainAddress.Line1);
        }

        [Fact]
        public void ReferencedObject()
        {
            var typeMap = new SerializationTypeMap(context);
            typeMap.RegisterType(typeof(Explicit.Sibling), "Sibling");
            typeMap.RegisterType(typeof(Explicit.Address), "Address", new Explicit.AddressSerializer());
            var serializer = new FudgeSerializer(context, typeMap);

            var bob = new Explicit.Sibling { Name = "Bob" };
            var shirley = new Explicit.Sibling { Name = "Shirley" };
            bob.Siblings.Add(shirley);                          // We don't reciprocate yet as that would generate a cycle

            var msgs = serializer.SerializeToMsgs(bob);

            var bob2 = (Explicit.Sibling)serializer.Deserialize(msgs);
            Assert.NotSame(bob, bob2);
            Assert.Equal(1, bob2.Siblings.Count);
            Assert.NotSame(shirley, bob2.Siblings[0]);
            Assert.Equal("Shirley", bob2.Siblings[0].Name);
        }

        [Fact]
        public void CircularReference()
        {
            var typeMap = new SerializationTypeMap(context);
            typeMap.RegisterType(typeof(Explicit.Sibling), "Sibling");
            typeMap.RegisterType(typeof(Explicit.Address), "Address", new Explicit.AddressSerializer());
            var serializer = new FudgeSerializer(context, typeMap);

            var bob = new Explicit.Sibling { Name = "Bob" };
            var shirley = new Explicit.Sibling { Name = "Shirley" };
            bob.Siblings.Add(shirley);
            shirley.Siblings.Add(bob);                          // Create our cycle

            var msgs = serializer.SerializeToMsgs(bob);

            var bob2 = (Explicit.Sibling)serializer.Deserialize(msgs);
            Assert.NotSame(bob, bob2);
            Assert.Equal(1, bob2.Siblings.Count);
            var shirley2 = (Explicit.Sibling)bob2.Siblings[0];
            Assert.NotSame(shirley, shirley2);
            Assert.Equal(1, shirley2.Siblings.Count);
            Assert.Same(bob2, shirley2.Siblings[0]);
        }

        [Fact]
        public void BaseTypesOutputAsWell_FRN43()
        {
            var serializer = new FudgeSerializer(context);

            var bob = new Explicit.Sibling { Name = "Bob" };

            var msgs = serializer.SerializeToMsgs(bob);

            var typeNames = msgs[0].GetAllValues<string>(FudgeSerializer.TypeIdFieldOrdinal);
            Assert.Equal(2, typeNames.Count);
            Assert.Equal("Fudge.Tests.Unit.Serialization.Explicit+Sibling", typeNames[0]);
            Assert.Equal("Fudge.Tests.Unit.Serialization.Explicit+Person", typeNames[1]);
        }

        [Fact]
        public void UsesFirstKnownType_FRN43()
        {
            var serializer = new FudgeSerializer(context);

            var bob = new Explicit.Sibling { Name = "Bob" };

            var msgs = serializer.SerializeToMsgs(bob);
            // Replace the object one
            msgs[0] = context.NewMessage(new Field(0, "Bibble"),
                                         new Field(0, "Fudge.Tests.Unit.Serialization.Explicit+Sibling"),
                                         new Field("name", "Bob"));
            var bob2 = (Explicit.Sibling)serializer.Deserialize(msgs);
            Assert.Equal("Bob", bob2.Name);
        }

        [Fact]
        public void InlineAttribute_FRN48()
        {
            var serializer = new FudgeSerializer(context);

            var parent = new InlineParent();

            var msgs = serializer.SerializeToMsgs(parent);
            var msg = msgs[0];

            Assert.Equal(FudgeMsgFieldType.Instance, msg.GetByName("In").Type);
            Assert.Equal(PrimitiveFieldTypes.SByteType, msg.GetByName("InForcedOut").Type);     // Reference collapses to byte
            Assert.Equal(PrimitiveFieldTypes.SByteType, msg.GetByName("Out").Type);
            Assert.Equal(FudgeMsgFieldType.Instance, msg.GetByName("OutForcedIn").Type);
        }

        [Fact]
        public void InlineThroughContext_FRN50()
        {
            var context2 = new FudgeContext();
            var parent = new InlineParent();

            // Check default
            var serializer = new FudgeSerializer(context2);
            var msg = serializer.SerializeToMsgs(parent)[0];
            Assert.Equal(PrimitiveFieldTypes.SByteType, msg.GetByName("Out").Type);

            // Inline
            context2.SetProperty(FudgeSerializer.InlineByDefault, true);
            serializer = new FudgeSerializer(context2);
            msg = serializer.SerializeToMsgs(parent)[0];
            Assert.Equal(FudgeMsgFieldType.Instance, msg.GetByName("Out").Type);

            // Don't inline
            context2.SetProperty(FudgeSerializer.InlineByDefault, false);
            serializer = new FudgeSerializer(context2);
            msg = serializer.SerializeToMsgs(parent)[0];
            Assert.Equal(PrimitiveFieldTypes.SByteType, msg.GetByName("Out").Type);
        }

        public class TemperatureRange
        {
            public double High { get; set; }
            public double Low { get; set; }
            public double Average { get; set; }
        }

        #region Inlining test classes

        [FudgeInline]
        private class Inlined
        {
            public bool Value { get; set; }
        }

        private class NotInlined
        {
            public bool Value { get; set; }
        }

        private class InlineParent
        {
            public InlineParent()
            {
                In = new Inlined();
                InForcedOut = new Inlined();
                Out = new NotInlined();
                OutForcedIn = new NotInlined();
            }

            public Inlined In { get; set; }

            [FudgeInline(false)]    // Override the type
            public Inlined InForcedOut { get; set; }

            public NotInlined Out { get; set; }

            [FudgeInline]    // Override the type
            public NotInlined OutForcedIn { get; set; }
        }

        #endregion
    }
}
