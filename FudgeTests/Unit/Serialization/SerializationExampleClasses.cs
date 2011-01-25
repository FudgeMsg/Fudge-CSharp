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
using Fudge.Serialization;

namespace Fudge.Tests.Unit.Serialization
{
    public static class Explicit
    {
        // The explicit classes implement serialization directly rather than using reflection

        // Note that Person is also used as the example in the docs for IFudgeSerializable.
        public class Person : IFudgeSerializable
        {
            public string Name { get; set; }
            public Address MainAddress { get; set; }

            public Person()
            {
            }

            #region IFudgeSerializable Members

            public virtual void Serialize(IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                msg.Add("name", Name);
                msg.AddIfNotNull("mainAddress", MainAddress);
            }

            public virtual void Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
            {
                foreach (IFudgeField field in msg)
                {
                    DeserializeField(deserializer, field);
                }
            }

            #endregion

            protected virtual bool DeserializeField(IFudgeDeserializer deserializer, IFudgeField field)
            {
                switch (field.Name)
                {
                    case "name":
                        Name = field.GetString();
                        return true;
                    case "mainAddress":
                        MainAddress = deserializer.FromField<Address>(field);
                        return true;
                }

                // Field not recognised
                return false;
            }
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

            public override void Serialize(IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                // Add our parent's fields
                base.Serialize(msg, serializer);

                // Now tag on ours
                msg.AddAll("siblings", siblings);
            }

            protected override bool DeserializeField(IFudgeDeserializer deserializer, IFudgeField field)
            {
                // Let the base process first
                if (base.DeserializeField(deserializer, field))
                    return true;

                // Now process our fields
                switch (field.Name)
                {
                    case "siblings":
                        siblings.Add(deserializer.FromField<Person>(field));
                        return true;
                }

                // Neither recognised the field
                return false;
            }
        }

        // Address is immutable, so has to use a surrogate (AddressSerializer)
        public class Address
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

            public string Line1 { get { return line1; } }
            public string Line2 { get { return line2; } }
            public string Zip { get { return zip; } }
        }

        public class AddressSerializer : IFudgeSerializationSurrogate
        {
            #region IFudgeSerializationSurrogate Members

            public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                var address = (Address)obj;
                msg.AddIfNotNull("line1", address.Line1);
                msg.AddIfNotNull("line2", address.Line2);
                msg.AddIfNotNull("zip", address.Zip);
            }

            public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
            {
                var address = new Address(msg.GetString("line1"),
                                          msg.GetString("line2"),
                                          msg.GetString("zip"));
                deserializer.Register(msg, address);
                return address;
            }

            #endregion
        }

        public sealed class Tick : IFudgeSerializable
        {
            public string Ticker { get; set; }
            public double Bid { get; set; }
            public double Offer { get; set; }

            #region IFudgeSerializable Members

            public void Serialize(IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                msg.Add("ticker", Ticker);
                msg.Add("bid", Bid);
                msg.Add("offer", Offer);
            }

            public void Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
            {
                Ticker = msg.GetString("ticker");
                Bid = msg.GetDouble("bid") ?? 0.0;
                Offer = msg.GetDouble("offer") ?? 0.0;
            }

            #endregion
        }
    }

    public static class Reflect
    {
        // The reflect classes are the same as Explicity, but use reflection rather than
        // implementing serialization themselves

        public class Person
        {
            public string Name { get; set; }
            public Address MainAddress { get; set; }
        }

        public class Sibling : Person
        {
            private readonly List<Person> siblings = new List<Person>();

            public IList<Person> Siblings
            {
                get { return siblings; }
            }
        }

        // Address is immutable, so has to use a surrogate
        public class Address
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

            public string Line1 { get { return line1; } }
            public string Line2 { get { return line2; } }
            public string Zip { get { return zip; } }
        }

        public class Tick
        {
            public string Ticker { get; set; }
            public double Bid { get; set; }
            public double Offer { get; set; }
        }
    }
}
