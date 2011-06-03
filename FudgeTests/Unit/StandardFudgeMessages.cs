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
using Fudge.Types;

namespace Fudge.Tests.Unit
{
    public class StandardFudgeMessages
    {
        internal static FudgeMsg CreateMessageAllNames(FudgeContext context)
        {
            FudgeMsg msg = context.NewMessage();

            msg.Add("boolean", true);
            msg.Add("Boolean", (object)false);
            msg.Add("byte", (sbyte)5);
            msg.Add("Byte", (object)((sbyte)5));
            short shortValue = ((short)sbyte.MaxValue) + 5;
            msg.Add("short", shortValue);
            msg.Add("Short", (object)(shortValue));
            int intValue = ((int)short.MaxValue) + 5;
            msg.Add("int", intValue);
            msg.Add("Integer", (object)(intValue));
            long longValue = ((long)int.MaxValue) + 5;
            msg.Add("long", longValue);
            msg.Add("Long", (object)(longValue));

            msg.Add("float", 0.5f);
            msg.Add("Float", (object)(0.5f));
            msg.Add("double", 0.27362);
            msg.Add("Double", (object)(0.27362));

            msg.Add("String", "Kirk Wylie");

            msg.Add("float array", new float[24]);
            msg.Add("double array", new double[273]);
            msg.Add("short array", new short[32]);
            msg.Add("int array", new int[83]);
            msg.Add("long array", new long[837]);

            msg.Add("indicator", IndicatorType.Instance);

            return msg;
        }

        internal static FudgeMsg CreateLargeMessage(FudgeContext context)
        {
            FudgeMsg msg = context.NewMessage();
            for (int i = short.MinValue - 1; i <= short.MaxValue + 1; i++)
            {
                msg.Add(string.Format("field{0}", i), "FRN-91");
            }
            return msg;
        }


        internal static FudgeMsg CreateMessageAllOrdinals(FudgeContext context)
        {
            FudgeMsg msg = context.NewMessage();

            msg.Add(1, true);
            msg.Add(2, (object)(false));
            msg.Add(3, (sbyte)5);
            msg.Add(4, (object)((sbyte)5));
            short shortValue = ((short)sbyte.MaxValue) + 5;
            msg.Add(5, shortValue);
            msg.Add(6, (object)(shortValue));
            int intValue = ((int)short.MaxValue) + 5;
            msg.Add(7, intValue);
            msg.Add(8, (object)(intValue));
            long longValue = ((long)int.MaxValue) + 5;
            msg.Add(9, longValue);
            msg.Add(10, (object)(longValue));

            msg.Add(11, 0.5f);
            msg.Add(12, (object)(0.5f));
            msg.Add(13, 0.27362);
            msg.Add(14, (object)(0.27362));

            msg.Add(15, "Kirk Wylie");

            msg.Add(16, new float[24]);
            msg.Add(17, new double[273]);

            return msg;
        }

        internal static FudgeMsg CreateMessageAllByteArrayLengths(FudgeContext context)
        {
            FudgeMsg msg = context.NewMessage();
            msg.Add("byte[4]", new byte[4]);
            msg.Add("byte[8]", new byte[8]);
            msg.Add("byte[16]", new byte[16]);
            msg.Add("byte[20]", new byte[20]);
            msg.Add("byte[32]", new byte[32]);
            msg.Add("byte[64]", new byte[64]);
            msg.Add("byte[128]", new byte[128]);
            msg.Add("byte[256]", new byte[256]);
            msg.Add("byte[512]", new byte[512]);

            msg.Add("byte[28]", new byte[28]);
            return msg;
        }

        public static FudgeMsg CreateMessageWithSubMsgs(FudgeContext context)
        {
            FudgeMsg inputMsg = context.NewMessage(
                                    new Field("sub1",
                                        new Field("bibble", "fibble"),
                                        new Field(827, "Blibble")),
                                    new Field("sub2",
                                        new Field("bibble9", 9837438),
                                        new Field(828, 82.77f)));
            return inputMsg;
        }
    }
}
