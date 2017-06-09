// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Security;
using Microsoft.Common.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Protocol {
    public class SecureStringJsonConverter : JsonConverter {
        public override bool CanConvert(Type objectType) => objectType == typeof(SecureString);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (typeof(SecureString) == objectType) {
                return ((string)reader.Value).ToSecureString();
            }

            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            JToken tok;
            if (value is SecureString) {
                tok = JToken.FromObject(((SecureString)value).ToUnsecureString());
            } else {
                tok = JToken.FromObject(value);
            }
            tok.WriteTo(writer);
        }
    }
}
