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

namespace Fudge.Tests.Unit.Serialization
{
    public class FudgeSerializerTest
    {
        private readonly FudgeContext context = new FudgeContext();

        [Fact]
        public void SimpleExampleWithSurrogate()
        {
            var typeMap = new SerializationTypeMap();
            typeMap.RegisterType(typeof(Address), "Address", c => new AddressSerializer());
            var serializer = new FudgeSerializer(context, typeMap);

            var address = new Address("Our House", "In the middle of our street", "MD1");
            var msgs = serializer.SerializeToMsgs(address);

            var address2 = (Address)serializer.Deserialize(msgs);

            Assert.Equal(address.Line1, address2.Line1);
            Assert.Equal(address.Line2, address2.Line2);
            Assert.Equal(address.Zip, address2.Zip);
        }

        [Fact]
        public void SimpleExampleWithIFudgeSerializable()
        {
            var typeMap = new SerializationTypeMap();
            typeMap.RegisterType(typeof(Tick), "Tick");
            var serializer = new FudgeSerializer(context, typeMap);

            var tick = new Tick { Ticker = "FOO", Bid = 12.3, Offer = 12.9 };
            var msgs = serializer.SerializeToMsgs(tick);

            var tick2 = (Tick)serializer.Deserialize(msgs);

            Assert.Equal(tick.Ticker, tick2.Ticker);
            Assert.Equal(tick.Bid, tick2.Bid);
            Assert.Equal(tick.Offer, tick2.Offer);
        }        

        [Fact]
        public void InlineObject()
        {
            var typeMap = new SerializationTypeMap();
            typeMap.RegisterType(typeof(Person), "Person");
            typeMap.RegisterType(typeof(Address), "Address", new AddressSerializer());
            var serializer = new FudgeSerializer(context, typeMap);

            var person = new Person{Name = "Bob", MainAddress = new Address ("Foo", "Bar", null) };
            var msgs = serializer.SerializeToMsgs(person);

            var person2 = (Person)serializer.Deserialize(msgs);
            Assert.NotSame(person.MainAddress, person2.MainAddress);
            Assert.Equal(person.MainAddress.Line1, person2.MainAddress.Line1);
        }

        [Fact]
        public void ReferencedObject()
        {
            var typeMap = new SerializationTypeMap();
            typeMap.RegisterType(typeof(Sibling), "Sibling");
            typeMap.RegisterType(typeof(Address), "Address", new AddressSerializer());
            var serializer = new FudgeSerializer(context, typeMap);

            var bob = new Sibling { Name = "Bob" };
            var shirley = new Sibling { Name = "Shirley" };
            bob.Siblings.Add(shirley);                          // We don't reciprocate yet as that would generate a cycle

            var msgs = serializer.SerializeToMsgs(bob);

            var bob2 = (Sibling)serializer.Deserialize(msgs);
            Assert.NotSame(bob, bob2);
            Assert.Equal(1, bob2.Siblings.Count);
            Assert.NotSame(shirley, bob2.Siblings[0]);
            Assert.Equal("Shirley", bob2.Siblings[0].Name);
        }

        [Fact]
        public void CircularReference()
        {
            var typeMap = new SerializationTypeMap();
            typeMap.RegisterType(typeof(Sibling), "Sibling");
            typeMap.RegisterType(typeof(Address), "Address", new AddressSerializer());
            var serializer = new FudgeSerializer(context, typeMap);

            var bob = new Sibling { Name = "Bob" };
            var shirley = new Sibling { Name = "Shirley" };
            bob.Siblings.Add(shirley);
            shirley.Siblings.Add(bob);                          // Create our cycle

            var msgs = serializer.SerializeToMsgs(bob);

            var bob2 = (Sibling)serializer.Deserialize(msgs);
            Assert.NotSame(bob, bob2);
            Assert.Equal(1, bob2.Siblings.Count);
            var shirley2 = (Sibling)bob2.Siblings[0];
            Assert.NotSame(shirley, shirley2);
            Assert.Equal(1, shirley2.Siblings.Count);
            Assert.Same(bob2, shirley2.Siblings[0]);
        }       
    }
}
