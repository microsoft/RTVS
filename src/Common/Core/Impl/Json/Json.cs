// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
            using (JsonReader reader = new JsonTextReader(new StringReader(json))) {
                reader.DateParseHandling = DateParseHandling.None;

                JToken t = JToken.Load(reader, settings);
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
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(null);
            jsonSerializer.CheckAdditionalContent = true;

            using (JsonTextReader reader = new JsonTextReader(new StringReader(value))) {
                reader.DateParseHandling = DateParseHandling.None;
                return jsonSerializer.Deserialize<T>(reader);
            }
        }
    }
}
