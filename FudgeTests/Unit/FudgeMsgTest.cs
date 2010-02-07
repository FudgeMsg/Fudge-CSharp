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
using Fudge.Types;

namespace Fudge.Tests.Unit
{
    public class FudgeMsgTest
    {
        private static readonly FudgeContext fudgeContext = new FudgeContext();

        [Fact]
        public void LookupByNameSingleValue()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);
            IFudgeField field = null;
            IList<IFudgeField> fields = null;

            field = msg.GetByName("boolean");
            Assert.NotNull(field);
            Assert.Equal(PrimitiveFieldTypes.BooleanType, field.Type);
            Assert.Equal(true, field.Value);
            Assert.Equal("boolean", field.Name);
            Assert.Null(field.Ordinal);

            field = msg.GetByName("Boolean");
            Assert.NotNull(field);
            Assert.Equal(PrimitiveFieldTypes.BooleanType, field.Type);
            Assert.Equal((object)false, field.Value);
            Assert.Equal("Boolean", field.Name);
            Assert.Null(field.Ordinal);

            fields = msg.GetAllByName("boolean");
            Assert.NotNull(fields);
            Assert.Equal(1, fields.Count);
            field = fields[0];
            Assert.NotNull(field);
            Assert.Equal(PrimitiveFieldTypes.BooleanType, field.Type);
            Assert.Equal(true, field.Value);
            Assert.Equal("boolean", field.Name);
            Assert.Null(field.Ordinal);

            // Check the indicator type specially
            Assert.Same(IndicatorType.Instance, msg.GetValue("indicator"));
        }

        [Fact]
        public void LookupByNameMultipleValues()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);
            IFudgeField field = null;
            IList<IFudgeField> fields = null;

            // Now add a second by name.
            msg.Add("boolean", true);

            field = msg.GetByName("boolean");
            Assert.NotNull(field);
            Assert.Equal(PrimitiveFieldTypes.BooleanType, field.Type);
            Assert.Equal(true, field.Value);
            Assert.Equal("boolean", field.Name);
            Assert.Null(field.Ordinal);

            fields = msg.GetAllByName("boolean");
            Assert.NotNull(fields);
            Assert.Equal(2, fields.Count);
            field = fields[0];
            Assert.NotNull(field);
            Assert.Equal(PrimitiveFieldTypes.BooleanType, field.Type);
            Assert.Equal(true, field.Value);
            Assert.Equal("boolean", field.Name);
            Assert.Null(field.Ordinal);

            field = fields[1];
            Assert.NotNull(field);
            Assert.Equal(PrimitiveFieldTypes.BooleanType, field.Type);
            Assert.Equal(true, field.Value);
            Assert.Equal("boolean", field.Name);
            Assert.Null(field.Ordinal);
        }

        [Fact]
        public void PrimitiveExactQueriesNamesMatch()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);

            Assert.Equal((sbyte)5, msg.GetSByte("byte"));
            Assert.Equal((sbyte)5, msg.GetSByte("Byte"));

            short shortValue = ((short)sbyte.MaxValue) + 5;
            Assert.Equal(shortValue, msg.GetShort("short"));
            Assert.Equal(shortValue, msg.GetShort("Short"));

            int intValue = ((int)short.MaxValue) + 5;
            Assert.Equal(intValue, msg.GetInt("int"));
            Assert.Equal(intValue, msg.GetInt("Integer"));

            long longValue = ((long)int.MaxValue) + 5;
            Assert.Equal(longValue, msg.GetLong("long"));
            Assert.Equal(longValue, msg.GetLong("Long"));

            Assert.Equal(0.5f, msg.GetFloat("float"));
            Assert.Equal(0.5f, msg.GetFloat("Float"));
            Assert.Equal(0.27362, msg.GetDouble("double"));
            Assert.Equal(0.27362, msg.GetDouble("Double"));

            Assert.Equal("Kirk Wylie", msg.GetString("String"));
        }

        [Fact]
        public void PrimitiveExactQueriesNamesNoMatch()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);

            Assert.Throws<OverflowException>(() => msg.GetSByte("int"));
            Assert.Throws<OverflowException>(() => msg.GetShort("int"));
            Assert.Equal(5, msg.GetInt("byte"));
            Assert.Equal(((long)short.MaxValue) + 5, msg.GetLong("int"));
            Assert.Equal(0.27362f, msg.GetFloat("double"));
            Assert.Equal(0.5, msg.GetDouble("float"));
        }

        [Fact]
        public void PrimitiveExactQueriesNoNames()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);

            Assert.Null(msg.GetSByte("foobar"));
            Assert.Null(msg.GetShort("foobar"));
            Assert.Null(msg.GetInt("foobar"));
            Assert.Null(msg.GetLong("foobar"));
            Assert.Null(msg.GetFloat("foobar"));
            Assert.Null(msg.GetDouble("foobar"));
            Assert.Null(msg.GetString("foobar"));
        }

        [Fact]
        public void AsQueriesToLongNames()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);

            Assert.Equal((long?)((sbyte)5), msg.GetLong("byte"));
            Assert.Equal((long?)((sbyte)5), msg.GetLong("Byte"));


            short shortValue = ((short)sbyte.MaxValue) + 5;
            Assert.Equal((long?)(shortValue), msg.GetLong("short"));
            Assert.Equal((long?)(shortValue), msg.GetLong("Short"));

            int intValue = ((int)short.MaxValue) + 5;
            Assert.Equal((long?)(intValue), msg.GetLong("int"));
            Assert.Equal((long?)(intValue), msg.GetLong("Integer"));

            long longValue = ((long)int.MaxValue) + 5;
            Assert.Equal((long?)(longValue), msg.GetLong("long"));
            Assert.Equal((long?)(longValue), msg.GetLong("Long"));

            Assert.Equal((long?)(0), msg.GetLong("float"));
            Assert.Equal((long?)(0), msg.GetLong("Float"));
            Assert.Equal((long?)(0), msg.GetLong("double"));
            Assert.Equal((long?)(0), msg.GetLong("Double"));
        }

        [Fact]
        public void GetValueTyped()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);
            long longValue = ((long)int.MaxValue) + 5;
            Assert.Equal(longValue, msg.GetValue<long>("long"));
            Assert.Equal(5, msg.GetValue<long>("byte"));
        }

        [Fact]
        public void AsQueriesToLongNoNames()        // TODO 2009-08-31 t0rx -- This test from Fudge-Java doesn't make sense
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);

            Assert.Null(msg.GetSByte("foobar"));
            Assert.Null(msg.GetShort("foobar"));
            Assert.Null(msg.GetInt("foobar"));
            Assert.Null(msg.GetLong("foobar"));
            Assert.Null(msg.GetFloat("foobar"));
            Assert.Null(msg.GetDouble("foobar"));
            Assert.Null(msg.GetString("foobar"));
        }

        // ------------

        [Fact]
        public void PrimitiveExactQueriesOrdinalsMatch()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllOrdinals(fudgeContext);

            Assert.Equal((sbyte)5, msg.GetSByte((short)3));
            Assert.Equal((sbyte)5, msg.GetSByte((short)4));

            short shortValue = ((short)sbyte.MaxValue) + 5;
            Assert.Equal(shortValue, msg.GetShort((short)5));
            Assert.Equal(shortValue, msg.GetShort((short)6));

            int intValue = ((int)short.MaxValue) + 5;
            Assert.Equal(intValue, msg.GetInt((short)7));
            Assert.Equal(intValue, msg.GetInt((short)8));

            long longValue = ((long)int.MaxValue) + 5;
            Assert.Equal(longValue, msg.GetLong((short)9));
            Assert.Equal(longValue, msg.GetLong((short)10));

            Assert.Equal(0.5f, msg.GetFloat((short)11));
            Assert.Equal(0.5f, msg.GetFloat((short)12));
            Assert.Equal(0.27362, msg.GetDouble((short)13));
            Assert.Equal(0.27362, msg.GetDouble((short)14));

            Assert.Equal("Kirk Wylie", msg.GetString((short)15));
        }

        [Fact]
        public void PrimitiveExactQueriesOrdinalsNoMatch()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllOrdinals(fudgeContext);

            Assert.Throws<OverflowException>(() => msg.GetSByte(7));
            Assert.Throws<OverflowException>(() => msg.GetShort(7));
            Assert.Throws<OverflowException>(() => msg.GetInt(9));
            Assert.Equal(((long)short.MaxValue) + 5, msg.GetLong(7));
            Assert.Equal(0.27362f, msg.GetFloat(13));
            Assert.Equal(0.5, msg.GetDouble(11));
        }

        [Fact]
        public void PrimitiveExactOrdinalsNoOrdinals()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllOrdinals(fudgeContext);

            Assert.Null(msg.GetSByte((short)100));
            Assert.Null(msg.GetShort((short)100));
            Assert.Null(msg.GetInt((short)100));
            Assert.Null(msg.GetLong((short)100));
            Assert.Null(msg.GetFloat((short)100));
            Assert.Null(msg.GetDouble((short)100));
            Assert.Null(msg.GetString((short)100));
        }

        [Fact]
        public void AsQueriesToLongOrdinals()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllOrdinals(fudgeContext);

            Assert.Equal((long)((sbyte)5), msg.GetLong((short)3));
            Assert.Equal((long)((sbyte)5), msg.GetLong((short)4));

            short shortValue = ((short)sbyte.MaxValue) + 5;
            Assert.Equal((long)(shortValue), msg.GetLong((short)5));
            Assert.Equal((long)(shortValue), msg.GetLong((short)6));

            int intValue = ((int)short.MaxValue) + 5;
            Assert.Equal((long)(intValue), msg.GetLong((short)7));
            Assert.Equal((long)(intValue), msg.GetLong((short)8));

            long longValue = ((long)int.MaxValue) + 5;
            Assert.Equal(longValue, msg.GetLong((short)9));
            Assert.Equal(longValue, msg.GetLong((short)10));

            Assert.Equal(0, msg.GetLong((short)11));
            Assert.Equal(0, msg.GetLong((short)12));
            Assert.Equal(0, msg.GetLong((short)13));
            Assert.Equal(0, msg.GetLong((short)14));
        }

        [Fact]
        public void ToByteArray()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);
            byte[] bytes = msg.ToByteArray();
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 10);
        }

        [Fact]
        public void LongInLongOut()
        {
            FudgeMsg msg = new FudgeMsg();

            msg.Add("test", (long)5);
            Assert.Equal((long)5, msg.GetLong("test"));
        }

        [Fact]
        public void FixedLengthByteArrays()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllByteArrayLengths(fudgeContext);
            Assert.Same(ByteArrayFieldType.Length4Instance, msg.GetByName("byte[4]").Type);
            Assert.Same(ByteArrayFieldType.Length8Instance, msg.GetByName("byte[8]").Type);
            Assert.Same(ByteArrayFieldType.Length16Instance, msg.GetByName("byte[16]").Type);
            Assert.Same(ByteArrayFieldType.Length20Instance, msg.GetByName("byte[20]").Type);
            Assert.Same(ByteArrayFieldType.Length32Instance, msg.GetByName("byte[32]").Type);
            Assert.Same(ByteArrayFieldType.Length64Instance, msg.GetByName("byte[64]").Type);
            Assert.Same(ByteArrayFieldType.Length128Instance, msg.GetByName("byte[128]").Type);
            Assert.Same(ByteArrayFieldType.Length256Instance, msg.GetByName("byte[256]").Type);
            Assert.Same(ByteArrayFieldType.Length512Instance, msg.GetByName("byte[512]").Type);

            Assert.Same(ByteArrayFieldType.VariableSizedInstance, msg.GetByName("byte[28]").Type);
        }

        [Fact]
        public void Minimization()
        {
            FudgeMsg msg = new FudgeMsg();
            msg.Add("int?", 17);

            Assert.Same(PrimitiveFieldTypes.SByteType, msg.GetByName("int?").Type);
        }

        [Fact]
        public void SecondaryTypes()
        {
            FudgeContext context = new FudgeContext();

            var guidType = new SecondaryFieldType<Guid, byte[]>(ByteArrayFieldType.Length16Instance, raw => new Guid(raw), value => value.ToByteArray());
            var typeDictionary = new FudgeTypeDictionary();
            typeDictionary.AddType(guidType);
            context.TypeDictionary = typeDictionary;

            Guid guid = Guid.NewGuid();
            FudgeMsg msg = new FudgeMsg(context);
            msg.Add("guid", guid);

            Assert.Same(ByteArrayFieldType.Length16Instance, msg.GetByName("guid").Type);

            Guid guid2 = msg.GetValue<Guid>("guid");
            Assert.Equal(guid, guid2);
        }

        [Fact]
        public void Iterable()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);
            int fieldCount = 0;
            foreach (IFudgeField field in msg)
            {
                fieldCount++;
            }
            Assert.Equal(msg.GetNumFields(), fieldCount);
        }

        [Fact]
        public void IterableContainer()
        {
            IFudgeFieldContainer msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);
            int fieldCount = 0;
            foreach (IFudgeField field in msg)
            {
                fieldCount++;
            }
            Assert.Equal(msg.GetNumFields(), fieldCount);
        }

        [Fact]
        public void AddingFieldContainerCopiesFields()
        {
            var msg = new FudgeMsg();

            // Add a normal sub-message (shouldn't copy)
            IFudgeFieldContainer sub1 = new FudgeMsg(new Field("age", 37));
            msg.Add("sub1", sub1);
            Assert.Same(sub1, msg.GetValue("sub1"));

            // Add a sub-message that isn't a FudgeMsg (should copy)
            IFudgeFieldContainer sub2 = (IFudgeFieldContainer)new Field("dummy", new Field("colour", "blue")).Value;
            Assert.IsNotType<FudgeMsg>(sub2);       // Just making sure
            msg.Add("sub2", sub2);
            Assert.NotSame(sub2, msg.GetValue("sub2"));
            Assert.IsType<FudgeMsg>(msg.GetValue("sub2"));
            Assert.Equal("blue", msg.GetMessage("sub2").GetString("colour"));
        }

        [Fact]
        public void GetAllNames()
        {
            var msg = new FudgeMsg();
            msg.Add("foo", 3);
            msg.Add("bar", 17);
            msg.Add("foo", 2);      // Deliberately do a duplicate
            var names = msg.GetAllFieldNames();
            Assert.Equal(2, names.Count);
            Assert.Contains("foo", names);
            Assert.Contains("bar", names);
        }

        [Fact]
        public void GetMessageMethodsFRN5()
        {
            var msg = StandardFudgeMessages.CreateMessageWithSubMsgs(fudgeContext);
            Assert.Null(msg.GetMessage(42));
            Assert.Null(msg.GetMessage("No Such Field"));
            Assert.True(msg.GetMessage("sub1") is IFudgeFieldContainer);
        }
    }
}
