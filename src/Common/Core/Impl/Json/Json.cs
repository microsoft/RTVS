// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Common.Core.Json {
    public static class Json {
        /// <summary>
        /// Like <see cref="JToken.Parse(string)"/>, but does not automatically deserialize strings that look like dates to <see cref="System.DateTime"/>.
        /// </summary>
        /// <remarks>
        /// Workaround for https://github.com/JamesNK/Newtonsoft.Json/issues/862.
        /// </remarks>
        public static JToken ParseToken(string json, JsonLoadSettings settings = null) {
            using (var reader = new JsonTextReader(new StringReader(json))) {
                reader.DateParseHandling = DateParseHandling.None;

                var t = JToken.Load(reader, settings);
                if (reader.Read() && reader.TokenType != JsonToken.Comment) {
                    throw new JsonReaderException("Additional text found in JSON string after parsing content.");
                }
                return t;
            }
        }

        /// <summary>
        /// Like <see cref="JsonConvert.DeserializeObject{T}(string)"/>, but does not automatically deserialize strings that look like dates to <see cref="System.DateTime"/>.
        /// </summary>
        /// <remarks>
        /// Workaround for  https://github.com/JamesNK/Newtonsoft.Json/issues/862.
        /// </remarks>
        public static T DeserializeObject<T>(string value) {
            var jsonSerializer = JsonSerializer.CreateDefault(null);
            jsonSerializer.CheckAdditionalContent = true;

            using (var reader = new JsonTextReader(new StringReader(value))) {
                reader.DateParseHandling = DateParseHandling.None;
                return jsonSerializer.Deserialize<T>(reader);
            }
        }

        /// <summary>
        /// Like <see cref="JsonConvert.DeserializeObject(string, Type)"/>, but does not automatically deserialize strings that look like dates to <see cref="System.DateTime"/>.
        /// </summary>
        /// <remarks>
        /// Workaround for  https://github.com/JamesNK/Newtonsoft.Json/issues/862.
        /// </remarks>
        public static object DeserializeObject(string value, Type type) {
            var jsonSerializer = JsonSerializer.CreateDefault(null);
            jsonSerializer.CheckAdditionalContent = true;
            try {
                using (var reader = new JsonTextReader(new StringReader(value))) {
                    reader.DateParseHandling = DateParseHandling.None;
                    return jsonSerializer.Deserialize(reader, type);
                }
            } catch(JsonReaderException) { }
            return null;
        }
    }
}
