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
using Xunit;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Fudge.Tests.Unit.Serialization.Reflection
{
    public class TypeDataTest
    {
        [Fact (Skip="Performance optimisation demonstration")]
        public void PerformanceComparison()
        {
            var obj = new SomeClass();

            PropertyInfo prop = obj.GetType().GetProperty("Val");

            // Create a delegate to get the property
            var getMethod = prop.GetGetMethod();
            Func<SomeClass, string> typedGetter = (Func<SomeClass, string>)Delegate.CreateDelegate(typeof(Func<SomeClass, string>), null, getMethod);
            Func<object, object> delegateGetter = o => typedGetter((SomeClass)o);

            // Create a dynamic method to get the property
            DynamicMethod dynamicMethod = new DynamicMethod("", typeof(object), new Type[] { typeof(object) }, typeof(SomeClass), true);
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, getMethod);
            if (prop.PropertyType.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Box, prop.PropertyType);
            }
            ilGenerator.Emit(OpCodes.Ret);
            var ilGetter = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));

            const int iterations = 10000000;

            var s = new Stopwatch();

            s.Reset();
            Console.Out.Write("PropertyInfo.GetValue: ");
            s.Start();
            for (int i = 0; i < iterations; i++)
            {
                prop.GetValue(obj, null);
            }
            s.Stop();
            Console.Out.WriteLine(s.ElapsedMilliseconds);

            s.Reset();
            Console.Out.Write("Delegate getter: ");
            s.Start();
            for (int i = 0; i < iterations; i++)
            {
                delegateGetter(obj);
            }
            s.Stop();
            Console.Out.WriteLine(s.ElapsedMilliseconds);

            s.Reset();
            Console.Out.Write("IL getter: ");
            s.Start();
            for (int i = 0; i < iterations; i++)
            {
                ilGetter(obj);
            }
            s.Stop();
            Console.Out.WriteLine("IL getter: " + s.ElapsedMilliseconds);
        }

        private class SomeClass
        {
            public string Val { get; set; }
        }
    }
}
