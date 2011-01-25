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
using System.Runtime.CompilerServices;

namespace Fudge.Encodings
{
    /// <summary>
    /// Classes to implement encoding and decoding Fudge message streams to various formats.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Fudge supports a generic notion of streams of hierarchical messages.  As well as the
    /// <a href="http://www.fudgemsg.org/display/FDG/Encoding+Specification">Fudge binary encoding</a>,
    /// it supports a number of other commonly-used formats.
    /// </para>
    /// <para>
    /// A particular encoding is implemented by providing two classes - one to read the stream
    /// and the other to write the stream, implementing <see cref="IFudgeStreamReader"/> and
    /// <see cref="IFudgeStreamWriter"/> respectively.  The currently supported formats are:
    /// <list type="table">
    /// <listheader>
    /// <term>Format</term>
    /// <description>Supporting classes</description>
    /// </listheader>
    /// <item><term>Fudge binary encoding</term><description><see cref="FudgeEncodedStreamReader"/>, <see cref="FudgeEncodedStreamWriter"/></description></item>
    /// <item><term>XML</term><description><see cref="FudgeXmlStreamReader"/>, <see cref="FudgeXmlStreamWriter"/></description></item>
    /// <item><term><see cref="FudgeMsg"/> objects</term><description><see cref="FudgeMsgStreamReader"/>, <see cref="FudgeMsgStreamWriter"/></description></item>
    /// <item><term>JSON</term><description><see cref="FudgeJSONStreamReader"/></description></item>
    /// </list>
    /// </para>
    /// <para>The <see cref="FudgeStreamMultiwriter"/> class also allows you to write to multiple encodings simultaneously.</para>
    /// </remarks>
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}
