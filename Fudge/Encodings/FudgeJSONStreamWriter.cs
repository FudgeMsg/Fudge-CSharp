/*
 * <!--
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
 * -->
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Fudge.Encodings
{
    /// <summary>
    /// Allows Fudge messages to be output as JSON text.
    /// </summary>
    /// <seealso cref="FudgeJSONStreamReader"/>
    public class FudgeJSONStreamWriter : IFudgeStreamWriter
    {
        // Implementation note - as we want to collapse fields of the same name and ordinal
        // into a JSON array, we can't output until we have the entire message
        private readonly FudgeContext context;
        private readonly TextWriter writer;
        private readonly string indentString = "   ";
        private readonly Stack<JSONObject> stack = new Stack<JSONObject>();

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="context">Context for the writer.</param>
        /// <param name="writer"><see cref="TextWriter"/> to receive the output.</param>
        public FudgeJSONStreamWriter(FudgeContext context, TextWriter writer)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (writer == null)
                throw new ArgumentNullException("writer");

            this.context = context;
            this.writer = writer;
        }

        private int Depth
        {
            get { return stack.Count; }
        }

        #region IFudgeStreamWriter Members

        /// <inheritdoc/>
        public void StartMessage()
        {
            if (Depth != 0)
                throw new InvalidOperationException("Attempt to start new message whilst in existing message");

            stack.Push(new JSONObject());
        }

        /// <inheritdoc/>
        public void StartSubMessage(string name, int? ordinal)
        {
            if (Depth == 0)
                throw new InvalidOperationException("Cannot start sub-message when not in an existing message");

            string fieldName = FormName(name, ordinal);
            var obj = new JSONObject();

            stack.Peek().AddField(fieldName, null, obj);
            stack.Push(obj);
        }

        /// <inheritdoc/>
        public void WriteField(string name, int? ordinal, FudgeFieldType type, object value)
        {
            if (Depth == 0)
                throw new InvalidOperationException("Cannot write a field when not in an existing message");

            if (value is IFudgeFieldContainer)
            {
                StartSubMessage(name, ordinal);
                WriteFields((IFudgeFieldContainer)value);
                EndSubMessage();
            }
            else
            {
                string fieldName = FormName(name, ordinal);
                stack.Peek().AddField(fieldName, type, value);
            }
        }

        /// <inheritdoc/>
        public void WriteFields(IEnumerable<IFudgeField> fields)
        {
            foreach (var field in fields)
                WriteField(field.Name, field.Ordinal, field.Type, field.Value);
        }

        /// <inheritdoc/>
        public void EndSubMessage()
        {
            if (Depth < 2)
                throw new InvalidOperationException("Cannot end sub-message when not in a sub-message");

            stack.Pop();
        }

        /// <inheritdoc/>
        public void EndMessage()
        {
            if (Depth == 0)
                throw new InvalidOperationException("Attempt to end message whilst not in message");
            if (Depth > 1)
                throw new InvalidOperationException("Attempt to end message whilst in sub-message");

            var obj = stack.Pop();
            WriteObject(obj, "");
        }

        #endregion

        private void WriteObject(JSONObject obj, string currentIndent)
        {
            // Already indented to correct place on entry
            writer.WriteLine(JSONConstants.BeginObject);
            string newIndent = currentIndent + indentString;

            for (int i = 0; i < obj.Fields.Count; i++)
            {
                var field = obj.Fields[i];
                if (i > 0)
                    writer.WriteLine(JSONConstants.ValueSeparator);

                writer.Write(newIndent + EscapeAndWrapString(field.Key) + " " + JSONConstants.NameSeparator + " ");

                if (field.Value.Count > 1)
                {
                    writer.WriteLine();
                    writer.Write(newIndent);
                    WriteArray(field.Value, newIndent);
                }
                else
                {
                    WriteValue(field.Value[0].Type, field.Value[0].Value, newIndent);
                }
            }
            if (obj.Fields.Count > 0)
                writer.WriteLine();

            writer.Write(currentIndent + JSONConstants.EndObject);
        }

        private void WriteArray(IList<JSONObject.TypedValue> values, string currentIndent)
        {
            // Already indented to correct place on entry
            writer.WriteLine(JSONConstants.BeginArray);         // We're already on a line so don't need the indent
            string newIndent = currentIndent + indentString;

            for (int i = 0; i < values.Count; i++)
            {
                var val = values[i];
                if (i > 0)
                    writer.WriteLine(JSONConstants.ValueSeparator);

                writer.Write(newIndent);
                WriteValue(val.Type, val.Value, newIndent);
            }
            if (values.Count > 0)
                writer.WriteLine();
            writer.Write(currentIndent + JSONConstants.EndArray);
        }

        private void WriteValue(FudgeFieldType type, object value, string currentIndent)
        {
            if (value == null)
            {
                writer.Write(JSONConstants.NullLiteral);
            }
            else if (type == null && value is JSONObject)           // Type will always be null for sub-objects, so this is a fast test compared to "is"
            {
                WriteObject((JSONObject)value, currentIndent);
            }
            else
            {
                type = context.TypeHandler.DetermineTypeFromValue(value);
                if (type == null)
                {
                    // Unknown type, so just treat it as a string
                    writer.Write(EscapeAndWrapString(value.ToString()));
                }
                else
                {
                    switch (type.TypeId)
                    {
                        case FudgeTypeDictionary.BOOLEAN_TYPE_ID:
                            writer.Write((bool)value ? JSONConstants.TrueLiteral : JSONConstants.FalseLiteral);
                            break;
                        case FudgeTypeDictionary.SBYTE_TYPE_ID:
                        case FudgeTypeDictionary.SHORT_TYPE_ID:
                        case FudgeTypeDictionary.INT_TYPE_ID:
                        case FudgeTypeDictionary.LONG_TYPE_ID:
                        case FudgeTypeDictionary.FLOAT_TYPE_ID:
                        case FudgeTypeDictionary.DOUBLE_TYPE_ID:
                            writer.Write(value.ToString());
                            break;
                        case FudgeTypeDictionary.BYTE_ARRAY_TYPE_ID:
                        case FudgeTypeDictionary.BYTE_ARR_4_TYPE_ID:
                        case FudgeTypeDictionary.BYTE_ARR_8_TYPE_ID:
                        case FudgeTypeDictionary.BYTE_ARR_16_TYPE_ID:
                        case FudgeTypeDictionary.BYTE_ARR_20_TYPE_ID:
                        case FudgeTypeDictionary.BYTE_ARR_32_TYPE_ID:
                        case FudgeTypeDictionary.BYTE_ARR_64_TYPE_ID:
                        case FudgeTypeDictionary.BYTE_ARR_128_TYPE_ID:
                        case FudgeTypeDictionary.BYTE_ARR_256_TYPE_ID:
                        case FudgeTypeDictionary.BYTE_ARR_512_TYPE_ID:
                        case FudgeTypeDictionary.SHORT_ARRAY_TYPE_ID:
                        case FudgeTypeDictionary.INT_ARRAY_TYPE_ID:
                        case FudgeTypeDictionary.LONG_ARRAY_TYPE_ID:
                        case FudgeTypeDictionary.FLOAT_ARRAY_TYPE_ID:
                        case FudgeTypeDictionary.DOUBLE_ARRAY_TYPE_ID:
                            WritePrimitiveArray((Array)value);
                            break;
                        case FudgeTypeDictionary.INDICATOR_TYPE_ID:
                            writer.Write(JSONConstants.NullLiteral);
                            break;
                        default:
                            // Anything else just treat as a string
                            writer.Write(EscapeAndWrapString(value.ToString()));
                            break;
                    }
                }
            }
        }

        private void WritePrimitiveArray(Array array)
        {
            writer.Write(JSONConstants.BeginArray);
            for (int i = 0; i < array.Length; i++)
            {
                if (i > 0)
                    writer.Write(JSONConstants.ValueSeparator + " ");

                writer.Write(array.GetValue(i).ToString());
            }
            writer.Write(JSONConstants.EndArray);
        }

        private string FormName(string name, int? ordinal)
        {
            // TODO 20100311 t0rx -- Handle ordinals and missing names
            return name;
        }

        private string EscapeAndWrapString(string s)
        {
            var sb = new StringBuilder((int)(s.Length * 1.25));
            sb.Append("\"");
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                switch (c)
                {
                    case '"':
                    case '\\':
                        sb.Append('\\').Append(c);
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        if (c < 0x20)
                        {
                            // Have to escape
                            sb.Append("\\u").Append(((int)c).ToString("X4"));
                        }
                        else
                        {
                            // Just leave as unicode
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");
            return sb.ToString();
        }       

        private class JSONObject
        {
            public readonly List<KeyValuePair<string, List<TypedValue>>> Fields = new List<KeyValuePair<string, List<TypedValue>>>();
            public readonly Dictionary<string, int> Indices = new Dictionary<string, int>();

            public void AddField(string name, FudgeFieldType type, object value)
            {
                int index;
                if (Indices.TryGetValue(name, out index))
                {
                    // Already got
                    Fields[index].Value.Add(new TypedValue(type, value));
                }
                else
                {
                    // New entry
                    var valueList = new List<TypedValue>();
                    valueList.Add(new TypedValue(type, value));
                    Indices[name] = Fields.Count;
                    Fields.Add(new KeyValuePair<string,List<TypedValue>>(name, valueList));
                }
            }

            public struct TypedValue
            {
                public readonly FudgeFieldType Type;
                public readonly object Value;
                
                public TypedValue(FudgeFieldType type, object value)
                {
                    this.Type = type;
                    this.Value = value;
                }

            }
        }
    }

    internal sealed class JSONConstants        // See http://www.ietf.org/rfc/rfc4627.txt?number=4627
    {
        public const char NameSeparator = ':';
        public const char ValueSeparator = ',';
        public const char BeginArray = '[';
        public const char EndArray = ']';
        public const char BeginObject = '{';
        public const char EndObject = '}';

        public const string NullLiteral = "null";
        public const string FalseLiteral = "false";
        public const string TrueLiteral = "true";
    }
}
