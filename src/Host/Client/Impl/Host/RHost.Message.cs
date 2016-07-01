// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        private class Message {
            public readonly string Id;
            public readonly string RequestId;
            public readonly string Name;

            public ConcurrentQueue<byte[]> Blobs;
            public readonly int ExpectedBlobs;

            private readonly JArray _body;
            private readonly int _argsOffset;

            public int ArgumentCount => _body.Count - _argsOffset;

            public Message(JToken token) {
                _body = token as JArray;
                var header = _body[0] as JArray;

                if (_body == null) {
                    throw ProtocolError($"Message must be an array:", token);
                }

                if(header == null) {
                    throw ProtocolError($"Message header must be an array:", header);
                }

                if (_body.Count < 1) {
                    throw ProtocolError($"Message must have form [[id, name, blob_count], ...]:", token);
                }

                if (header.Count < 3) {
                    throw ProtocolError($"Message header must have form [[id, name, blob_count], ...]:", header);
                }

                var id = header[0];
                if(id.Type != JTokenType.String) {
                    throw ProtocolError($"id must be {JTokenType.String}:", this);
                }
                Id = (string)id;

                var name = header[1];
                if (name.Type != JTokenType.String) {
                    throw ProtocolError($"name must be {JTokenType.String}:", this);
                }
                Name = (string)name;

                var blob = header[2];
                if (blob.Type != JTokenType.Integer) {
                    throw ProtocolError($"blob_count must be {JTokenType.Integer}:", this);
                }
                ExpectedBlobs = (int)blob;

                if (Name.StartsWithOrdinal(":")) {
                    if (header.Count < 4) {
                        throw ProtocolError($"Response message must have form [[id, name, blob_count, request_id, ...], ...]:", token);
                    }

                    var requestId = header[3];
                    if (requestId.Type != JTokenType.String) {
                        throw ProtocolError($"request_id must be {JTokenType.String}:", this);
                    }
                    RequestId = (string)requestId;
                }

                // header part is done
                _argsOffset = 1;

                Blobs = new ConcurrentQueue<byte[]>(); 
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
