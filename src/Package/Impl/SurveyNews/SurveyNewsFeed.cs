// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.VisualStudio.R.Package.SurveyNews {
    /// <summary>
    /// Represents the json data returned by the survey/news server.
    /// </summary>
    /// <remarks>
    /// Example JSON data returned by the server:
    /// {
    ///  "cannotvoteagain": [], 
    ///  "notvoted": [
    ///   "http://rtvs.azurewebsites.net/news/141", 
    ///   "http://rtvs.azurewebsites.net/news/41", 
    ///  ], 
    ///  "canvoteagain": [
    ///   "http://rtvs.azurewebsites.net/news/51"
    ///  ]
    /// }
    /// </remarks>
    internal class SurveyNewsFeed {
        /// <summary>
        /// Cookie found.
        /// </summary>
        [JsonProperty("cannotvoteagain")]
        public string[] CannotVoteAgainUrls { get; set; }

        /// <summary>
        /// Cookie not found.
        /// </summary>
        [JsonProperty("notvoted")]
        public string[] NotVotedUrls { get; set; }

        /// <summary>
        /// Cookie found, but multiple votes are allowed.
        /// </summary>
        [JsonProperty("canvoteagain")]
        public string[] CanVoteAgainUrls { get; set; }
    }
}
