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
            msg.Add((object)false, "Boolean");
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

            return msg;
        }

        protected internal static FudgeMsg CreateMessageAllOrdinals()
        {
            FudgeMsg msg = new FudgeMsg();

            msg.Add(true, (short)1);
            msg.Add((object)(false), (short)2);
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
        }

        [Fact]
        public void LookupByNameMultipleValues()
        {
            FudgeMsg msg = CreateMessageAllNames();
            IFudgeField field = null;
            List<IFudgeField> fields = null;

            // Now add a second by name.
            msg.Add(false, "boolean");

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
            Assert.Equal(false, field.Value);
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

            Assert.Null(msg.GetByte("int"));
            Assert.Null(msg.GetShort("int"));
            Assert.Null(msg.GetInt("byte"));
            Assert.Null(msg.GetLong("int"));
            Assert.Null(msg.GetFloat("double"));
            Assert.Null(msg.GetDouble("float"));
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

            Assert.Equal((long?)((byte)5), msg.GetAsLong("byte"));
            Assert.Equal((long?)((byte)5), msg.GetAsLong("Byte"));


            short shortValue = ((short)byte.MaxValue) + 5;
            Assert.Equal((long?)(shortValue), msg.GetAsLong("short"));
            Assert.Equal((long?)(shortValue), msg.GetAsLong("Short"));

            int intValue = ((int)short.MaxValue) + 5;
            Assert.Equal((long?)(intValue), msg.GetAsLong("int"));
            Assert.Equal((long?)(intValue), msg.GetAsLong("Integer"));

            long longValue = ((long)int.MaxValue) + 5;
            Assert.Equal((long?)(longValue), msg.GetAsLong("long"));
            Assert.Equal((long?)(longValue), msg.GetAsLong("Long"));

            Assert.Equal((long?)(0), msg.GetAsLong("float"));
            Assert.Equal((long?)(0), msg.GetAsLong("Float"));
            Assert.Equal((long?)(0), msg.GetAsLong("double"));
            Assert.Equal((long?)(0), msg.GetAsLong("Double"));
        }

        [Fact]
        public void AsQueriesToLongNoNames()        // TODO: 20090831 (t0rx): This test doesn't make sense
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

            Assert.Null(msg.GetByte((short)7));
            Assert.Null(msg.GetShort((short)7));
            Assert.Null(msg.GetInt((short)9));
            Assert.Null(msg.GetLong((short)7));
            Assert.Null(msg.GetFloat((short)13));
            Assert.Null(msg.GetDouble((short)11));
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

            Assert.Equal((long)((byte)5), msg.GetAsLong((short)3));
            Assert.Equal((long)((byte)5), msg.GetAsLong((short)4));

            short shortValue = ((short)byte.MaxValue) + 5;
            Assert.Equal((long)(shortValue), msg.GetAsLong((short)5));
            Assert.Equal((long)(shortValue), msg.GetAsLong((short)6));

            int intValue = ((int)short.MaxValue) + 5;
            Assert.Equal((long)(intValue), msg.GetAsLong((short)7));
            Assert.Equal((long)(intValue), msg.GetAsLong((short)8));

            long longValue = ((long)int.MaxValue) + 5;
            Assert.Equal(longValue, msg.GetAsLong((short)9));
            Assert.Equal(longValue, msg.GetAsLong((short)10));

            Assert.Equal(0, msg.GetAsLong((short)11));
            Assert.Equal(0, msg.GetAsLong((short)12));
            Assert.Equal(0, msg.GetAsLong((short)13));
            Assert.Equal(0, msg.GetAsLong((short)14));
        }

        [Fact]
        public void ToByteArray()
        {
            FudgeMsg msg = CreateMessageAllNames();
            byte[] bytes = msg.ToByteArray();
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 10);
        }

        /* TODO: 20090831 (t0rx): I reckon this should pass
        [Fact]
        public void SmallLongComesOut()
        {
            FudgeMsg msg = new FudgeMsg();

            msg.Add((long)5, "test");
            Assert.Equal((long)5, msg.GetLong("test"));
        }
         */
    }
}
