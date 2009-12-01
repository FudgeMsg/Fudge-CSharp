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
using Fudge.Serialization;

namespace Fudge.Tests.Unit.Serialization
{
    public class TrackingFudgeMsgTest
    {
        [Fact]
        public void SimpleExample()
        {
            var msg = new FudgeMsg(new Field(1, "Hello"),
                                   new Field(2, "world"));

            var tracker = new TrackingFudgeMsg(msg);
            var value1 = tracker.GetString(2);
            Assert.Equal("world", value1);

            var leftovers = tracker.GetUnvisitedFields();
            Assert.Equal(1, leftovers.Count);
            Assert.Equal("Hello", leftovers[0].Value);
        }

        [Fact]
        public void DoubleCountingFields()
        {
            var msg = new FudgeMsg(new Field("f1", "Hello"),
                                   new Field("f2", "world"));

            var tracker = new TrackingFudgeMsg(msg);
            var value1 = tracker.GetString("f1");
            Assert.Equal("Hello", value1);
            var value2 = tracker.GetValue("f1");

            var leftovers = tracker.GetUnvisitedFields();
            Assert.Equal(1, leftovers.Count);
            Assert.Equal("world", leftovers[0].Value);
        }

        [Fact]
        public void MissingFields()
        {
            var msg = new FudgeMsg(new Field(1, "Hello"));

            var tracker = new TrackingFudgeMsg(msg);
            var value1 = tracker.GetString(3);
            Assert.Null(value1);

            var leftovers = tracker.GetUnvisitedFields();
            Assert.Equal(1, leftovers.Count);
            Assert.Equal("Hello", leftovers[0].Value);
        }

        [Fact]
        public void FieldsConstructorWorks()
        {
            var msg = new TrackingFudgeMsg(new Field("f1", "Hello"),
                                           new Field("f2", "world"));
            Assert.Equal(2, msg.GetNumFields());
            Assert.Equal(2, msg.GetUnvisitedFields().Count);
        }
    }
}
