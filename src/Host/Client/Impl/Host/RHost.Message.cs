// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class Message {
            public readonly string Id;
            public readonly string RequestId;
            public readonly string Name;

            private readonly JArray _body;
            private readonly int _argsOffset;

            public int ArgumentCount => _body.Count - _argsOffset;

            public Message(JToken token) {
                _body = token as JArray;
                if (_body == null) {
                    throw ProtocolError($"Message must be an array:", token);
                }
                if (_body.Count < 2) {
                    throw ProtocolError($"Message must have form [id, name, ...]:", token);
                }

                Id = GetString(0, "id");
                Name = GetString(1, "name");
                _argsOffset = 2;

                if (Name == ":") {
                    if (_body.Count < 4) {
                        throw ProtocolError($"Response message must have form [id, ':', request_id, name, ...]:", token);
                    }

                    RequestId = GetString(0, "request_id");
                    Name = GetString(1, "request_name");
                    _argsOffset += 2;
                }
            }

            public override string ToString() {
                return _body.ToString();
            }

            public void ExpectArguments(int count) {
                if (ArgumentCount != count) {
                    throw ProtocolError($"{count} arguments expected:", this);
                }
            }

            public void ExpectArguments(int min, int max) {
                if (ArgumentCount < min) {
                    throw ProtocolError($"At least {min} arguments expected:", this);
                } else if (ArgumentCount > max) {
                    throw ProtocolError($"At most {max} arguments expected:", this);
                }
            }

            public JToken this[int i] {
                get {
                    return _body[_argsOffset + i];
                }
            }

            public JToken GetArgument(int i, string name, JTokenType expectedType) {
                var arg = this[i];
                if (arg.Type != expectedType) {
                    throw ProtocolError($"{name} must be {expectedType}:", this);
                }
                return arg;
            }

            public JToken GetArgument(int i, string name, JTokenType expectedType1, JTokenType expectedType2) {
                var arg = this[i];
                if (arg.Type != expectedType1 && arg.Type != expectedType2) {
                    throw ProtocolError($"{name} must be {expectedType1} or {expectedType2}:", this);
                }
                return arg;
            }

            public string GetString(int i, string name, bool allowNull = false) {
                var arg = allowNull ? GetArgument(i, name, JTokenType.String, JTokenType.Null) : GetArgument(i, name, JTokenType.String);
                if (arg.Type == JTokenType.Null) {
                    return null;
                }
                return (string)arg;
            }

            public int GetInt32(int i, string name) {
                var arg = GetArgument(i, name, JTokenType.Integer);
                return (int)arg;
            }

            public bool GetBoolean(int i, string name) {
                var arg = GetArgument(i, name, JTokenType.Boolean);
                return (bool)arg;
            }

            public TEnum GetEnum<TEnum>(int i, string name, string[] names) {
                var arg = GetArgument(i, name, JTokenType.Integer, JTokenType.String);

                if (arg.Type == JTokenType.Integer) {
                    return (TEnum)(object)(int)arg;
                }

                int n = Array.IndexOf(names, (string)arg);
                if (n < 0) {
                    throw ProtocolError($"Argument #{i} must be integer, or one of: {string.Join(", ", names)}:", this);
                }

                return (TEnum)(object)n;
            }
        }
    }
}
