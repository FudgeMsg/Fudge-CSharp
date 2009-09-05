/**
* Copyright (C) 2009 - 2009 by OpenGamma Inc.
*
* Please see distribution for license.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using OpenGamma.Fudge.Types;

namespace OpenGamma.Fudge.Tests.Unit
{
    public class FudgeMsgTest
    {
        protected internal static FudgeMsg CreateMessageAllNames()
        {
            FudgeMsg msg = new FudgeMsg();

            msg.Add(true, "boolean");
            msg.Add((object)true, "Boolean");
            msg.Add((byte)5, "byte");
            msg.Add((object)((byte)5), "Byte");
            short shortValue = ((short)byte.MaxValue) + 5;
            msg.Add(shortValue, "short");
            msg.Add((object)(shortValue), "Short");
            int intValue = ((int)short.MaxValue) + 5;
            msg.Add(intValue, "int");
            msg.Add((object)(intValue), "Integer");
            long longValue = ((long)int.MaxValue) + 5;
            msg.Add(longValue, "long");
            msg.Add((object)(longValue), "Long");

            msg.Add(0.5f, "float");
            msg.Add((object)(0.5f), "Float");
            msg.Add(0.27362, "double");
            msg.Add((object)(0.27362), "Double");

            msg.Add("Kirk Wylie", "String");

            msg.Add(new float[24], "float array");
            msg.Add(new double[273], "double array");
            msg.Add(new short[32], "short array");
            msg.Add(new int[83], "int array");
            msg.Add(new long[873], "long array");

            msg.Add(IndicatorType.Instance, "indicator");

            return msg;
        }

        protected internal static FudgeMsg CreateMessageAllOrdinals()
        {
            FudgeMsg msg = new FudgeMsg();

            msg.Add(true, (short)1);
            msg.Add((object)(true), (short)2);
            msg.Add((byte)5, (short)3);
            msg.Add((object)((byte)5), (short)4);
            short shortValue = ((short)byte.MaxValue) + 5;
            msg.Add(shortValue, (short)5);
            msg.Add((object)(shortValue), (short)6);
            int intValue = ((int)short.MaxValue) + 5;
            msg.Add(intValue, (short)7);
            msg.Add((object)(intValue), (short)8);
            long longValue = ((long)int.MaxValue) + 5;
            msg.Add(longValue, (short)9);
            msg.Add((object)(longValue), (short)10);

            msg.Add(0.5f, (short)11);
            msg.Add((object)(0.5f), (short)12);
            msg.Add(0.27362, (short)13);
            msg.Add((object)(0.27362), (short)14);

            msg.Add("Kirk Wylie", (short)15);

            msg.Add(new float[24], (short)16);
            msg.Add(new double[273], (short)17);

            return msg;
        }

        [Fact]
        public void LookupByNameSingleValue()
        {
            FudgeMsg msg = CreateMessageAllNames();
            IFudgeField field = null;
            List<IFudgeField> fields = null;

            field = msg.GetByName("boolean");
            Assert.NotNull(field);
            Assert.Equal(PrimitiveFieldTypes.BooleanType, field.Type);
            Assert.Equal(true, field.Value);
            Assert.Equal("boolean", field.Name);
            Assert.Null(field.Ordinal);

            field = msg.GetByName("Boolean");
            Assert.NotNull(field);
            Assert.Equal(PrimitiveFieldTypes.BooleanType, field.Type);
            Assert.Equal((object)true, field.Value);
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
            FudgeMsg msg = CreateMessageAllNames();
            IFudgeField field = null;
            List<IFudgeField> fields = null;

            // Now add a second by name.
            msg.Add(true, "boolean");

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
            FudgeMsg msg = CreateMessageAllNames();

            Assert.Equal((byte)5, msg.GetByte("byte"));
            Assert.Equal((byte)5, msg.GetByte("Byte"));

            short shortValue = ((short)byte.MaxValue) + 5;
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
            FudgeMsg msg = CreateMessageAllNames();

            Assert.Throws<OverflowException>(() => msg.GetByte("int"));
            Assert.Throws<OverflowException>(() => msg.GetShort("int"));
            Assert.Equal(5, msg.GetInt("byte"));
            Assert.Equal(((long)short.MaxValue) + 5, msg.GetLong("int"));
            Assert.Equal(0.27362f, msg.GetFloat("double"));
            Assert.Equal(0.5, msg.GetDouble("float"));
        }

        [Fact]
        public void PrimitiveExactQueriesNoNames()
        {
            FudgeMsg msg = CreateMessageAllNames();

            Assert.Null(msg.GetByte("foobar"));
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
            FudgeMsg msg = CreateMessageAllNames();

            Assert.Equal((long?)((byte)5), msg.GetLong("byte"));
            Assert.Equal((long?)((byte)5), msg.GetLong("Byte"));


            short shortValue = ((short)byte.MaxValue) + 5;
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
        public void AsQueriesToLongNoNames()        // TODO t0rx 2009-08-31 -- This test from Fudge-Java doesn't make sense
        {
            FudgeMsg msg = CreateMessageAllNames();

            Assert.Null(msg.GetByte("foobar"));
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
            FudgeMsg msg = CreateMessageAllOrdinals();

            Assert.Equal((byte)5, msg.GetByte((short)3));
            Assert.Equal((byte)5, msg.GetByte((short)4));

            short shortValue = ((short)byte.MaxValue) + 5;
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
            FudgeMsg msg = CreateMessageAllOrdinals();

            Assert.Throws<OverflowException>(()=>msg.GetByte(7));
            Assert.Throws<OverflowException>(() => msg.GetShort(7));
            Assert.Throws<OverflowException>(() => msg.GetInt(9));
            Assert.Equal(((long)short.MaxValue) + 5, msg.GetLong(7));
            Assert.Equal(0.27362f, msg.GetFloat(13));
            Assert.Equal(0.5, msg.GetDouble(11));
        }

        [Fact]
        public void PrimitiveExactOrdinalsNoOrdinals()
        {
            FudgeMsg msg = CreateMessageAllOrdinals();

            Assert.Null(msg.GetByte((short)100));
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
            FudgeMsg msg = CreateMessageAllOrdinals();

            Assert.Equal((long)((byte)5), msg.GetLong((short)3));
            Assert.Equal((long)((byte)5), msg.GetLong((short)4));

            short shortValue = ((short)byte.MaxValue) + 5;
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
            FudgeMsg msg = CreateMessageAllNames();
            byte[] bytes = msg.ToByteArray();
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 10);
        }

        [Fact]
        public void LongInLongOut()
        {
            FudgeMsg msg = new FudgeMsg();

            msg.Add((long)5, "test");
            Assert.Equal((long)5, msg.GetLong("test"));
        }

        [Fact]
        public void IndicatorBehaviour()
        {
            FudgeMsg inputMsg = new FudgeMsg();

            inputMsg.Add(false, 1);
            Assert.Same(IndicatorType.Instance, inputMsg.GetByOrdinal(1).Value);
            Assert.Equal(false, inputMsg.GetBoolean(1));

            inputMsg.Add((byte)0, 2);
            Assert.Same(IndicatorType.Instance, inputMsg.GetByOrdinal(2).Value);
            Assert.Equal((byte)0, inputMsg.GetByte(2));

            inputMsg.Add((short)0, 3);
            Assert.Same(IndicatorType.Instance, inputMsg.GetByOrdinal(3).Value);
            Assert.Equal((short)0, inputMsg.GetShort(3));

            inputMsg.Add((int)0, 4);
            Assert.Same(IndicatorType.Instance, inputMsg.GetByOrdinal(4).Value);
            Assert.Equal((int)0, inputMsg.GetInt(4));

            inputMsg.Add((long)0, 5);
            Assert.Same(IndicatorType.Instance, inputMsg.GetByOrdinal(5).Value);
            Assert.Equal((long)0, inputMsg.GetLong(5));

            inputMsg.Add(0.0f, 6);
            Assert.Same(IndicatorType.Instance, inputMsg.GetByOrdinal(6).Value);
            Assert.Equal(0.0f, inputMsg.GetFloat(6));

            inputMsg.Add(0.0, 7);
            Assert.Same(IndicatorType.Instance, inputMsg.GetByOrdinal(7).Value);
            Assert.Equal(0.0, inputMsg.GetDouble(7));
            
            inputMsg.Add("", 8);
            Assert.Same(IndicatorType.Instance, inputMsg.GetByOrdinal(8).Value);
            Assert.Equal("", inputMsg.GetString(8));
        }
    }
}
