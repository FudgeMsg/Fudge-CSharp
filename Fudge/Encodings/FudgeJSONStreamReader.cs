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
using System.Diagnostics;
using Fudge.Types;
using System.Globalization;

namespace Fudge.Encodings
{
    /// <summary>
    /// Implementation of <see cref="IFudgeStreamReader"/> that reads JSON messages
    /// </summary>
    /// <remarks>
    /// Parsing based on definition of syntax at http://www.json.org/ as of 2009-12-18.
    /// </remarks>
    public class FudgeJSONStreamReader : FudgeStreamReaderBase
    {
        private readonly FudgeContext context;
        private readonly TextReader reader;
        private Token nextToken;
        private bool done = false;
        private Stack<State> stack = new Stack<State>();

        /// <summary>
        /// Constructs a <see cref="FudgeJSONStreamReader"/> on a given <see cref="TextReader"/>.
        /// </summary>
        /// <param name="context">Context to control behaviours.</param>
        /// <param name="reader"><see cref="TextReader"/> providing the data.</param>
        public FudgeJSONStreamReader(FudgeContext context, TextReader reader)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (reader == null)
                throw new ArgumentNullException("reader");

            this.context = context;
            this.reader = reader;
        }

        /// <summary>
        /// Constructs a <see cref="FudgeJSONStreamReader"/> using a <c>string</c> for the underlying data.
        /// </summary>
        /// <param name="context">Context to control behaviours.</param>
        /// <param name="text">Text containing JSON message.</param>
        /// <example>This example shows a simple JSON string being converted into a <see cref="FudgeMsg"/> object:
        /// <code>
        /// string json = @"{""name"" : ""fred""}";
        /// FudgeMsg msg = new FudgeJSONStreamReader(json).ReadToMsg();
        /// </code>
        /// </example>
        public FudgeJSONStreamReader(FudgeContext context, string text)
            : this(context, new StringReader(text))
        {
        }

        #region IFudgeStreamReader Members

        /// <inheritdoc/>
        public override bool HasNext
        {
            get
            {
                return !done && (PeekNextToken() != Token.EOF);
            }
        }

        /// <inheritdoc/>
        public override FudgeStreamElement MoveNext()
        {
            Token token;

            if (Depth == 0)
            {
                // Have to start with a {
                token = GetNextToken();
                if (token != Token.BeginObject)
                {
                    throw new FudgeParseException("Expected '{' at start of JSON stream");
                }
                stack.Push(State.InObject);
                CurrentElement = FudgeStreamElement.MessageStart;
                return CurrentElement;
            }

            token = GetNextToken();

            if (token == Token.EOF)
                throw new FudgeParseException("Premature EOF in JSON stream");

            var top = stack.Peek();
            if (top.IsInArray)
            {
                // If we're directly inside an array then the field name was outside
                FieldName = top.ArrayFieldName;
            }
            else
            {
                if (token == Token.EndObject)
                {
                    HandleObjectEnd(token);
                    return CurrentElement;
                }

                // Not at the end of an object, so must be a field
                if (token.Type != TokenType.String)
                    throw new FudgeParseException("Expected field name in JSON stream, got " + token + "");
                FieldName = token.StringData;

                token = GetNextToken();
                if (token != Token.NameSeparator)
                    throw new FudgeParseException("Expected ':' in JSON stream for field \"" + FieldName + "\", got " + token + "");

                token = GetNextToken();
            }

            HandleValue(token);
            return CurrentElement;
        }

        #endregion

        private void HandleObjectEnd(Token token)
        {
            stack.Pop();
            if (stack.Count == 0)
            {
                CurrentElement = FudgeStreamElement.MessageEnd;
            }
            else
            {
                CurrentElement = FudgeStreamElement.SubmessageFieldEnd;

                var top = stack.Peek();
                if (top.IsInArray)
                {
                    SkipCommaPostValue(token.ToString());
                }
            }
        }

        private void HandleValue(Token token)
        {
            if (token.IsSimpleValue)
            {
                HandleSimpleValue(token);
                SkipCommaPostValue(token.ToString());
            }
            else if (token == Token.BeginObject)
            {
                stack.Push(State.InObject);
                CurrentElement = FudgeStreamElement.SubmessageFieldStart;
            }
            else if (token == Token.BeginArray)
            {
                stack.Push(new State(FieldName));

                // Recurse to deal with real value
                HandleValue(GetNextToken());
            }
            else
                throw new FudgeParseException("Unexpected token \"" + token + "\" in JSON stream when looking for a value");
        }

        private int Depth { get { return stack.Count; } }

        private void HandleSimpleValue(Token token)
        {
            CurrentElement = FudgeStreamElement.SimpleField;
            if (token.Type == TokenType.String)
            {
                FieldType = StringFieldType.Instance;
                FieldValue = token.StringData;
            }
            else if (token.Type == TokenType.Integer)
            {
                FieldType = PrimitiveFieldTypes.IntType;
                FieldValue = token.IntData;
            }
            else if (token.Type == TokenType.Double)
            {
                FieldType = PrimitiveFieldTypes.DoubleType;
                FieldValue = token.DoubleData;
            }
            else if (token == Token.True)
            {
                FieldType = PrimitiveFieldTypes.BooleanType;
                FieldValue = true;
            }
            else if (token == Token.False)
            {
                FieldType = PrimitiveFieldTypes.BooleanType;
                FieldValue = false;
            }
            else if (token == Token.Null)       // REVIEW 2009-12-18 t0rx -- Is it right to map a JSON null to Indicator?
            {
                FieldType = IndicatorFieldType.Instance;
                FieldValue = IndicatorType.Instance;
            }
            else
            {
                Debug.Assert(false, "Unknown simple value token " + token);
            }
        }

        private void SkipCommaPostValue(string context)
        {
            while (true)
            {
                var token = PeekNextToken();
                if (token == Token.ValueSeparator)
                {
                    // Skip past it
                    GetNextToken();
                    return;
                }
                else if (token == Token.EndArray)
                {
                    // Skip it and pop the stack as well
                    GetNextToken();
                    stack.Pop();

                    // Go round again as there may be a comma or } next
                }
                else if (token == Token.EndObject)
                {
                    return;
                }
                else
                {
                    throw new FudgeParseException("Expected , or } after " + context);
                }
            }
        }

        private Token PeekNextToken()
        {
            if (nextToken == null)
                nextToken = ParseNextToken();
            return nextToken;
        }

        private Token GetNextToken()
        {
            if (nextToken == null)
                return ParseNextToken();
            else
            {
                var result = nextToken;
                nextToken = null;
                return result;
            }
        }

        private Token ParseNextToken()
        {
            while (true)
            {
                int next = reader.Read();
                if (next == -1)
                {
                    return Token.EOF;
                }

                switch (next)
                {
                    case JSONConstants.BeginObject:
                        return Token.BeginObject;
                    case JSONConstants.EndObject:
                        return Token.EndObject;
                    case JSONConstants.BeginArray:
                        return Token.BeginArray;
                    case JSONConstants.EndArray:
                        return Token.EndArray;
                    case '"':
                        return ParseString();
                    case JSONConstants.NameSeparator:
                        return Token.NameSeparator;
                    case JSONConstants.ValueSeparator:
                        return Token.ValueSeparator;
                    case ' ':
                    case '\r':
                    case '\n':
                    case '\f':
                    case '\b':
                    case '\t':
                        // Ignore white space
                        break;
                    default:
                        return ParseLiteral((char)next);
                }
            }
        }

        private Token ParseLiteral(char startChar)
        {
            string literal = startChar + ReadLiteral();

            if (literal == JSONConstants.TrueLiteral)
                return Token.True;
            if (literal == JSONConstants.FalseLiteral)
                return Token.False;
            if (literal == JSONConstants.NullLiteral)
                return Token.Null;

            var token = ParseNumber(literal);
            if (token.Type != TokenType.Error)
                return token;

            // No idea what it is then
            throw new FudgeParseException("Unrecognised JSON token \"" + literal + "\"");
        }

        private string ReadLiteral()
        {
            var sb = new StringBuilder();
            bool done = false;
            while (!done)
            {
                int next = reader.Peek();
                switch (next)
                {
                    case -1:        // EOF
                        done = true;
                        break;
                    case ' ':
                    case '\b':
                    case '\t':
                    case '\r':
                    case '\f':
                    case '\n':
                    case '\\':
                    case JSONConstants.BeginObject:
                    case JSONConstants.EndObject:
                    case JSONConstants.ValueSeparator:
                    case JSONConstants.BeginArray:
                    case JSONConstants.EndArray:
                        done = true;
                        break;
                    default:
                        sb.Append((char)next);
                        reader.Read();
                        break;
                }
            }
            return sb.ToString();
        }

        private Token ParseNumber(string literal)
        {
            bool isDouble = literal.Contains('.') || literal.Contains('e') || literal.Contains('E');
            try
            {
                if (isDouble)
                {
                    return new Token(TokenType.Double, literal) { DoubleData = double.Parse(literal) };
                }
                else
                {
                    return new Token(TokenType.Integer, literal) { IntData = int.Parse(literal) };
                }
            }
            catch (FormatException)
            {
                return new Token(TokenType.Error, literal);
            }
        }

        private Token ParseString()
        {
            StringBuilder sb = new StringBuilder();

            // We've already had the opening quote
            while (true)
            {
                int next = reader.Read();
                if (next == -1)
                {
                    return Token.EOF;
                }
                char nextChar = (char)next;

                switch (nextChar)
                {
                    case '"':
                        // We're done
                        return new Token(TokenType.String, sb.ToString()) { StringData = sb.ToString() };
                    case '\\':
                        {
                            // Escaped char
                            next = reader.Read();
                            if (next == -1)
                            {
                                return Token.EOF;
                            }
                            switch (next)
                            {
                                case '"':
                                case '\\':
                                case '/':
                                    sb.Append((char)next);
                                    break;
                                case 'b':
                                    sb.Append('\b');
                                    break;
                                case 'f':
                                    sb.Append('\f');
                                    break;
                                case 'n':
                                    sb.Append('\n');
                                    break;
                                case 'r':
                                    sb.Append('\r');
                                    break;
                                case 't':
                                    sb.Append('\t');
                                    break;
                                case 'u':
                                    sb.Append(ReadUnicode());
                                    break;
                            }
                            break;
                        }
                    default:
                        sb.Append(nextChar);
                        break;
                }
            }
        }

        private char ReadUnicode()
        {
            char[] buffer = new char[4];
            if (reader.Read(buffer, 0, 4) != 4)
                throw new FudgeParseException("Premature EOF whilst trying to read \\u in string");

            StringBuilder sb = new StringBuilder();
            sb.Append(buffer);
            int val = int.Parse(sb.ToString(), NumberStyles.HexNumber);
            return (char)val;
        }

        private class State
        {
            private readonly string arrayFieldName;

            public State(string arrayFieldName)
            {
                this.arrayFieldName = arrayFieldName;
            }

            public State()
            {
                this.arrayFieldName = null;
            }

            public bool IsInArray { get { return arrayFieldName != null; } }

            public string ArrayFieldName { get { return arrayFieldName; } }

            public static readonly State InObject = new State();
        }

        private class Token
        {
            private readonly string toString;

            public Token(TokenType type, string toString)
            {
                this.Type = type;
                this.toString = toString;
            }

            public Token(TokenType type, char ch) : this(type, ch.ToString())
            {
            }

            public TokenType Type { get; private set; }

            public string StringData { get; set; }

            public int IntData { get; set; }

            public double DoubleData { get; set; }

            public bool IsSimpleValue
            {
                get
                {
                    return Type == TokenType.Double ||
                           Type == TokenType.Integer ||
                           Type == TokenType.String ||
                           this == Token.True ||
                           this == Token.False ||
                           this == Token.Null;
                }
            }

            public override string ToString()
            {
                return toString;
            }

            public static readonly Token EOF = new Token(TokenType.Special, "EOF");
            public static readonly Token BeginObject = new Token(TokenType.Special, JSONConstants.BeginObject);
            public static readonly Token EndObject = new Token(TokenType.Special, JSONConstants.EndObject);
            public static readonly Token BeginArray = new Token(TokenType.Special, JSONConstants.BeginArray);
            public static readonly Token EndArray = new Token(TokenType.Special, JSONConstants.EndArray);
            public static readonly Token NameSeparator = new Token(TokenType.Special, JSONConstants.NameSeparator);
            public static readonly Token ValueSeparator = new Token(TokenType.Special, JSONConstants.ValueSeparator);
            public static readonly Token True = new Token(TokenType.Special, JSONConstants.TrueLiteral);
            public static readonly Token False = new Token(TokenType.Special, JSONConstants.FalseLiteral);
            public static readonly Token Null = new Token(TokenType.Special, JSONConstants.NullLiteral);
        }

        enum TokenType
        {
            Special,
            String,
            Integer,
            Double,
            Error
        }
    }
}
