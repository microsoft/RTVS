// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.SurveyNews {
    internal interface ISurveyNewsService {
        /// <summary>
        /// Contact the survey/news server and opens a web page to a news item, if one is available.
        /// </summary>
        /// <param name="forceCheck">
        /// Check the server regardless of policy setting and last check date.
        /// This is used when the check is initiated by the user via a menu item.
        /// In that case, a web page will always open:
        /// the available news item page, the news index page, or a local error page
        /// if the feed could not be retrieved.
        /// </param>
        /// <returns></returns>
        Task CheckSurveyNewsAsync(bool forceCheck);
    }
}
