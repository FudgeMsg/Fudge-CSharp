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
using System.IO;

namespace OpenGamma.Fudge.Tests.Unit
{
    public class FudgeMsgFormatterTest
    {
        /// <summary>
        /// Will output a <see cref="FudgeMsg"/> to <see cref="Console.Out"/> so that you can visually
        /// examine it.
        /// </summary>
        [Fact]
        public void OutputToStdoutAllNames()
        {
            Console.Out.WriteLine("FudgeMsgFormatterTest.OutputToStdoutAllNames()");
            FudgeMsg msg = FudgeMsgTest.CreateMessageAllNames();
            msg.Add(FudgeMsgTest.CreateMessageAllNames(), "Sub Message", (short)9999);
            new FudgeMsgFormatter(Console.Out).Format(msg);
        }

        /// <summary>
        /// Will output a <see cref="FudgeMsg"/> to <see cref="Console.Out"/> so that you can visually
        /// examine it.
        /// </summary>
        public void OutputToStdoutAllOrdinals()
        {
            Console.Out.WriteLine("FudgeMsgFormatterTest.OutputToStdoutAllOrdinals()");
            FudgeMsg msg = FudgeMsgTest.CreateMessageAllOrdinals();
            msg.Add(FudgeMsgTest.CreateMessageAllOrdinals(), "Sub Message", (short)9999);
            new FudgeMsgFormatter(Console.Out).Format(msg);
        }
    }
}
