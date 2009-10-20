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
using Xunit;
using OpenGamma.Fudge.Serialization;

namespace OpenGamma.Fudge.Tests.Unit.Serialization
{
    public class FudgeSerializerTest
    {
        [Fact]
        public void SimpleExampleWithSurrogate()
        {
            var typeMap = new SerializationTypeMap();
            typeMap.RegisterType(typeof(Address), "Address", new AddressSerializer());
            var serializer = new FudgeSerializer(typeMap);

            var address = new Address ("Our House", "In the middle of our street", "MD1");
            var msg = serializer.Serialize(address);

            var address2 = (Address)serializer.Deserialize(msg);

            Assert.Equal(address.Line1, address2.Line1);
            Assert.Equal(address.Line2, address2.Line2);
            Assert.Equal(address.Zip, address2.Zip);
        }

        [Fact]
        public void SimpleExampleWithIFudgeSerializable()
        {
            var typeMap = new SerializationTypeMap();
            typeMap.RegisterType(typeof(Tick), "Tick");
            var serializer = new FudgeSerializer(typeMap);

            var tick = new Tick { Ticker = "FOO", Bid = 12.3, Offer = 12.9 };
            var msg = serializer.Serialize(tick);

            var tick2 = (Tick)serializer.Deserialize(msg);

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
            var serializer = new FudgeSerializer(typeMap);

            var person = new Person{Name = "Bob", MainAddress = new Address ("Foo", "Bar", null) };
            var msg = serializer.Serialize(person);

            var person2 = (Person)serializer.Deserialize(msg);
            Assert.NotSame(person.MainAddress, person2.MainAddress);
            Assert.Equal(person.MainAddress.Line1, person2.MainAddress.Line1);
        }

        [Fact]
        public void ReferencedObject()
        {
            var typeMap = new SerializationTypeMap();
            typeMap.RegisterType(typeof(Sibling), "Sibling");
            typeMap.RegisterType(typeof(Address), "Address", new AddressSerializer());
            var serializer = new FudgeSerializer(typeMap);

            var bob = new Sibling { Name = "Bob" };
            var shirley = new Sibling { Name = "Shirley" };
            bob.Siblings.Add(shirley);                          // We don't reciprocate yet as that would generate a cycle

            var msg = serializer.Serialize(bob);

            var bob2 = (Sibling)serializer.Deserialize(msg);
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
            var serializer = new FudgeSerializer(typeMap);

            var bob = new Sibling { Name = "Bob" };
            var shirley = new Sibling { Name = "Shirley" };
            bob.Siblings.Add(shirley);
            shirley.Siblings.Add(bob);                          // Create our cycle

            var msg = serializer.Serialize(bob);

            var bob2 = (Sibling)serializer.Deserialize(msg);
            Assert.NotSame(bob, bob2);
            Assert.Equal(1, bob2.Siblings.Count);
            var shirley2 = (Sibling)bob2.Siblings[0];
            Assert.NotSame(shirley, shirley2);
            Assert.Equal(1, shirley2.Siblings.Count);
            Assert.Same(bob2, shirley2.Siblings[0]);

            new FudgeMsgFormatter(Console.Out).Format(msg);
        }

        #region Example classes
        public class Address            // We'll also make this immutable to show how that works
        {
            private readonly string line1;
            private readonly string line2;
            private readonly string zip;

            public Address(string line1, string line2, string zip)
            {
                this.line1 = line1;
                this.line2 = line2;
                this.zip = zip;
            }

            public string Line1 { get {return line1;} }
            public string Line2 { get { return line2; } }
            public string Zip { get { return zip; } }
        }

        public class AddressSerializer : IFudgeSerializationSurrogate
        {
            #region IFudgeSerializationSurrogate Members

            public void Serialize(object obj, FudgeMsg msg, IFudgeSerializationContext context)
            {
                var address = (Address)obj;
                msg.Add("line1", address.Line1);
                msg.Add("line2", address.Line2);
                msg.AddIfNotNull("zip", address.Zip);
            }

            public object Deserialize(FudgeMsg msg, int dataVersion, IFudgeDeserializationContext context)
            {
                var address = new Address(msg.GetString("line1"),
                                          msg.GetString("line2"),
                                          msg.GetString("zip"));
                return address;
            }

            #endregion
        }

        public class Person : IFudgeSerializable
        {
            public string Name { get; set; }
            public Address MainAddress { get; set; }

            public Person()
            {
            }

            #region IFudgeSerializable Members

            public virtual void Serialize(FudgeMsg msg, IFudgeSerializationContext context)
            {
                msg.Add("name", Name);
                msg.AddIfNotNull("mainAddress", context.AsSubMsg(MainAddress));
            }

            public virtual void Deserialize(FudgeMsg msg, int dataVersion, IFudgeDeserializationContext context)
            {
                Name = msg.GetString("name");
                MainAddress = context.FromField<Address>(msg.GetByName("mainAddress"));
            }

            #endregion
        }

        public class Sibling : Person
        {
            private readonly List<Person> siblings = new List<Person>();

            public Sibling()
            {
            }
            
            public IList<Person> Siblings
            {
                get { return siblings; }
            }

            public override void Serialize(FudgeMsg msg, IFudgeSerializationContext context)
            {
                base.Serialize(msg, context);
                msg.AddAll("siblings", context.AllAsRefs(siblings));
            }

            public override void Deserialize(FudgeMsg msg, int dataVersion, IFudgeDeserializationContext context)
            {
                base.Deserialize(msg, dataVersion, context);
                siblings.AddRange(context.AllFromFields<Person>(msg.GetAllByName("siblings")));
            }
        }

        public class Tick : IFudgeSerializable
        {
            public string Ticker { get; set; }
            public double Bid { get; set; }
            public double Offer { get; set; }

            #region IFudgeSerializable Members

            public void Serialize(FudgeMsg msg, IFudgeSerializationContext context)
            {
                msg.Add("ticker", Ticker);
                msg.Add("bid", Bid);
                msg.Add("offer", Offer);
            }

            public void Deserialize(FudgeMsg msg, int dataVersion, IFudgeDeserializationContext context)
            {
                Ticker = msg.GetString("ticker");
                Bid = msg.GetDouble("bid") ?? 0.0;
                Offer = msg.GetDouble("offer") ?? 0.0;
            }

            #endregion
        }

        #endregion
    }
}
