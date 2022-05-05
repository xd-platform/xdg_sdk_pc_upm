using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TapTap.TapDB.PC.Utils
{
    public static class TapDBJson
    {
        public static object Deserialize(string json)
        {
            // save the string for debug information
            return json == null ? null : Parser.Parse(json);
        }

        private sealed class Parser : IDisposable {
            private const string WordBreak = "{}[],:\"";

            private static bool IsWordBreak(char c) {
                return char.IsWhiteSpace(c) || WordBreak.IndexOf(c) != -1;
            }

            private enum Token {
                None,
                CurlyOpen,
                CurlyClose,
                SquaredOpen,
                SquaredClose,
                Colon,
                Comma,
                String,
                Number,
                True,
                False,
                Null
            };

            private StringReader _json;

            private Parser(string jsonString) {
                _json = new StringReader(jsonString);
            }

            public static object Parse(string jsonString) {
                using (var instance = new Parser(jsonString)) {
                    return instance.ParseValue();
                }
            }

            public void Dispose() {
                _json.Dispose();
                _json = null;
            }

            private Dictionary<string, object> ParseObject() {
                var table = new Dictionary<string, object>();

                // ditch opening brace
                _json.Read();

                // {
                while (true) {
                    switch (NextToken) {
                    case Token.None:
                        return null;
                    case Token.Comma:
                        continue;
                    case Token.CurlyClose:
                        return table;
                    default:
                        // name
                        var name = ParseString();
                        if (name == null) {
                            return null;
                        }

                        // :
                        if (NextToken != Token.Colon) {
                            return null;
                        }
                        // ditch the colon
                        _json.Read();

                        // value
                        table[name] = ParseValue();
                        break;
                    }
                }
            }

            private List<object> ParseArray() {
                var array = new List<object>();

                // ditch opening bracket
                _json.Read();

                // [
                var parsing = true;
                while (parsing) {
                    var nextToken = NextToken;

                    switch (nextToken) {
                    case Token.None:
                        return null;
                    case Token.Comma:
                        continue;
                    case Token.SquaredClose:
                        parsing = false;
                        break;
                    default:
                        var value = ParseByToken(nextToken);

                        array.Add(value);
                        break;
                    }
                }

                return array;
            }

            private object ParseValue() {
                var nextToken = NextToken;
                return ParseByToken(nextToken);
            }

            private object ParseByToken(Token token) {
                switch (token) {
                case Token.String:
                    return ParseString();
                case Token.Number:
                    return ParseNumber();
                case Token.CurlyOpen:
                    return ParseObject();
                case Token.SquaredOpen:
                    return ParseArray();
                case Token.True:
                    return true;
                case Token.False:
                    return false;
                case Token.Null:
                    return null;
                default:
                    return null;
                }
            }

            private string ParseString() {
                var s = new StringBuilder();

                // ditch opening quote
                _json.Read();

                var parsing = true;
                while (parsing) {

                    if (_json.Peek() == -1) {
                        break;
                    }

                    var c = NextChar;
                    switch (c) {
                    case '"':
                        parsing = false;
                        break;
                    case '\\':
                        if (_json.Peek() == -1) {
                            parsing = false;
                            break;
                        }

                        c = NextChar;
                        switch (c) {
                        case '"':
                        case '\\':
                        case '/':
                            s.Append(c);
                            break;
                        case 'b':
                            s.Append('\b');
                            break;
                        case 'f':
                            s.Append('\f');
                            break;
                        case 'n':
                            s.Append('\n');
                            break;
                        case 'r':
                            s.Append('\r');
                            break;
                        case 't':
                            s.Append('\t');
                            break;
                        case 'u':
                            var hex = new char[4];

                            for (int i=0; i< 4; i++) {
                                hex[i] = NextChar;
                            }

                            s.Append((char) Convert.ToInt32(new string(hex), 16));
                            break;
                        }
                        break;
                    default:
                        s.Append(c);
                        break;
                    }
                }

                return s.ToString();
            }

            private object ParseNumber() {
                var number = NextWord;

                if (number.IndexOf('.') == -1) {
                    long.TryParse(number, out var parsedInt);
                    return parsedInt;
                }

                double.TryParse(number, out var parsedDouble);
                return parsedDouble;
            }

            private void EatWhitespace() {
                while (char.IsWhiteSpace(PeekChar)) {
                    _json.Read();

                    if (_json.Peek() == -1) {
                        break;
                    }
                }
            }

            private char PeekChar => Convert.ToChar(_json.Peek());

            private char NextChar => Convert.ToChar(_json.Read());

            private string NextWord {
                get {
                    var word = new StringBuilder();

                    while (!IsWordBreak(PeekChar)) {
                        word.Append(NextChar);

                        if (_json.Peek() == -1) {
                            break;
                        }
                    }

                    return word.ToString();
                }
            }

            private Token NextToken {
                get {
                    EatWhitespace();

                    if (_json.Peek() == -1) {
                        return Token.None;
                    }

                    switch (PeekChar) {
                    case '{':
                        return Token.CurlyOpen;
                    case '}':
                        _json.Read();
                        return Token.CurlyClose;
                    case '[':
                        return Token.SquaredOpen;
                    case ']':
                        _json.Read();
                        return Token.SquaredClose;
                    case ',':
                        _json.Read();
                        return Token.Comma;
                    case '"':
                        return Token.String;
                    case ':':
                        return Token.Colon;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                        return Token.Number;
                    }

                    switch (NextWord) {
                    case "false":
                        return Token.False;
                    case "true":
                        return Token.True;
                    case "null":
                        return Token.Null;
                    }

                    return Token.None;
                }
            }
        }

        /// <summary>
        /// Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
        public static string Serialize(object obj) {
            return Serializer.Serialize(obj);
        }

        private sealed class Serializer {
            private readonly StringBuilder _builder;

            private Serializer() {
                _builder = new StringBuilder();
            }

            public static string Serialize(object obj) {
                var instance = new Serializer();

                instance.SerializeValue(obj);

                return instance._builder.ToString();
            }

            private void SerializeValue(object value) {
                IList asList;
                IDictionary asDict;
                string asStr;

                if (value == null) {
                    _builder.Append("null");
                } else if ((asStr = value as string) != null) {
                    SerializeString(asStr);
                } else if (value is bool b) {
                    _builder.Append(b ? "true" : "false");
                } else if ((asList = value as IList) != null) {
                    SerializeArray(asList);
                } else if ((asDict = value as IDictionary) != null) {
                    SerializeObject(asDict);
                } else if (value is char c) {
                    SerializeString(new string(c, 1));
                } else {
                    SerializeOther(value);
                }
            }

            private void SerializeObject(IDictionary obj) {
                var first = true;

                _builder.Append('{');

                foreach (var e in obj.Keys) {
                    if (!first) {
                        _builder.Append(',');
                    }

                    SerializeString(e.ToString());
                    _builder.Append(':');

                    SerializeValue(obj[e]);

                    first = false;
                }

                _builder.Append('}');
            }

            private void SerializeArray(IList anArray) {
                _builder.Append('[');

                var first = true;

                foreach (var obj in anArray) {
                    if (!first) {
                        _builder.Append(',');
                    }

                    SerializeValue(obj);

                    first = false;
                }

                _builder.Append(']');
            }

            private void SerializeString(string str) {
                _builder.Append('\"');

                var charArray = str.ToCharArray();
                foreach (var c in charArray) {
                    switch (c) {
                    case '"':
                        _builder.Append("\\\"");
                        break;
                    case '\\':
                        _builder.Append("\\\\");
                        break;
                    case '\b':
                        _builder.Append("\\b");
                        break;
                    case '\f':
                        _builder.Append("\\f");
                        break;
                    case '\n':
                        _builder.Append("\\n");
                        break;
                    case '\r':
                        _builder.Append("\\r");
                        break;
                    case '\t':
                        _builder.Append("\\t");
                        break;
                    default:
                        var codepoint = Convert.ToInt32(c);
                        if ((codepoint >= 32) && (codepoint <= 126)) {
                            _builder.Append(c);
                        } else {
                            _builder.Append("\\u");
                            _builder.Append(codepoint.ToString("x4"));
                        }
                        break;
                    }
                }

                _builder.Append('\"');
            }

            private void SerializeOther(object value)
            {
                switch (value)
                {
                    // NOTE: decimals lose precision during serialization.
                    // They always have, I'm just letting you know.
                    // Previously floats and doubles lost precision too.
                    case float f:
                        _builder.Append(f.ToString("R"));
                        break;
                    case int _:
                    case uint _:
                    case long _:
                    case sbyte _:
                    case byte _:
                    case short _:
                    case ushort _:
                    case ulong _:
                        _builder.Append(value);
                        break;
                    case double _:
                    case decimal _:
                        _builder.Append(Convert.ToDouble(value).ToString("R"));
                        break;
                    default:
                        SerializeString(value.ToString());
                        break;
                }
            }
        }
    }
}