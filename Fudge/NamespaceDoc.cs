/*
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
using System.Runtime.CompilerServices;

namespace Fudge
{
    /// <summary>
    /// This is the main namespace for Fudge, containing the <c>FudgeMsg</c> and <c>FudgeContext</c> classes.  See below for an example.
    /// </summary>
    /// <example>
    /// The following example shows some simple operations with messages.
    /// <code>
    /// var context = new FudgeContext();
    ///
    /// // Create a message
    /// var msg = new FudgeMsg(new Field("name", "Eric"),
    ///                        new Field("age", 14),
    ///                        new Field("address",
    ///                            new Field("line1", "29 Acacia Road"),
    ///                            new Field("city", "London")));
    ///
    /// // Serialise it
    /// var stream = new MemoryStream();
    /// context.Serialize(msg, stream);
    ///
    /// // Get the raw bytes
    /// var bytes = stream.ToArray();
    ///
    /// // Deserialise it
    /// var msg2 = context.Deserialize(bytes).Message;
    ///
    /// // Get some data
    /// int age = msg2.GetInt("age") ?? 0;
    /// </code>
    /// </example>
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}
