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
using System.IO;

namespace Fudge
{
    /// <summary>
    /// Allows for pretty-printing of <see cref="FudgeMsg"/> instances.
    /// </summary>
    public class FudgeMsgFormatter
    {

        public const int DEFAULT_INDENT = 2;
        private readonly TextWriter writer;
        private readonly int indent;
        private readonly string indentText;

        public FudgeMsgFormatter(TextWriter textWriter)
            : this(textWriter, DEFAULT_INDENT)
        {
        }

        public FudgeMsgFormatter(TextWriter writer, int indent)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("Must specify a valid writer for output.");
            }
            if (indent < 0)
            {
                throw new ArgumentOutOfRangeException("Indent must not be negative.");
            }
            this.writer = writer;
            this.indent = indent;
            this.indentText = ComposeIndentText(indent);
        }

        public TextWriter Writer
        {
            get
            {
                return writer;
            }
        }

        public int Indent
        {
            get
            {
                return indent;
            }
        }

        public void Format(IFudgeFieldContainer msg)
        {
            Format(msg, 0);
        }

        protected void Format(IFudgeFieldContainer msg, int depth)
        {
            if (msg == null)
            {
                return;
            }
            IList<IFudgeField> fields = msg.GetAllFields();
            IList<string> fieldSpecs = new List<string>(fields.Count);
            int maxFieldSpecWidth = -1;
            int maxTypeNameWidth = -1;
            for (int i = 0; i < fields.Count; i++)
            {
                IFudgeField field = fields[i];
                string fieldSpec = GetFieldSpec(field, i, depth);
                maxFieldSpecWidth = Math.Max(maxFieldSpecWidth, fieldSpec.Length);
                maxTypeNameWidth = Math.Max(maxTypeNameWidth, GetTypeName(field.Type).Length);
                fieldSpecs.Add(fieldSpec);
            }
            for (int i = 0; i < fields.Count; i++)
            {
                IFudgeField field = fields[i];
                string fieldSpec = fieldSpecs[i];
                Format(field, i, depth, fieldSpec, maxFieldSpecWidth, maxTypeNameWidth);
            }
        }

        protected int GetFieldSpecWidth(IFudgeField field, int index, int depth)
        {
            return GetFieldSpec(field, index, depth).Length;
        }

        protected void Format(IFudgeField field, int index, int depth, string fieldSpec, int maxFieldSpecWidth, int maxTypeNameWidth)
        {
            if (field == null)
            {
                throw new ArgumentNullException("Cannot format a null field");
            }
            Writer.Write(fieldSpec);
            int nWritten = fieldSpec.Length;
            int requiredSize = maxFieldSpecWidth + 1;
            for (int i = nWritten; i <= requiredSize; i++)
            {
                Writer.Write(' ');
                nWritten++;
            }
            string typeName = GetTypeName(field.Type);
            Writer.Write(typeName);
            nWritten += typeName.Length;
            requiredSize = requiredSize + maxTypeNameWidth + 1;
            for (int i = nWritten; i <= requiredSize; i++)
            {
                Writer.Write(' ');
                nWritten++;
            }
            if (field.Value is FudgeMsg)
            {
                Writer.WriteLine();
                FudgeMsg msgValue = (FudgeMsg)field.Value;
                Format(msgValue, depth + 1);
            }
            else
            {
                Writer.Write(field.Value);
                Writer.WriteLine();
            }
            Writer.Flush();
        }

        protected string GetFieldSpec(IFudgeField field, int index, int depth)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < depth; i++)
            {
                sb.Append(indentText);
            }
            sb.Append(index);
            sb.Append("-");
            if (field.Ordinal != null)
            {
                sb.Append("(").Append(field.Ordinal).Append(")");
                if (field.Name != null)
                {
                    sb.Append(" ");
                }
            }
            if (field.Name != null)
            {
                sb.Append(field.Name);
            }
            return sb.ToString();
        }

        protected string ComposeIndentText(int indent)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(' ', indent);
            return sb.ToString();
        }

        protected string GetTypeName(FudgeFieldType type)
        {
            if (type == null)
            {
                throw new NullReferenceException("Must specify a type.");
            }
            return type.CSharpType.Name;
        }
    }
}
