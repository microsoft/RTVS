// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Json;
using Newtonsoft.Json;

namespace Microsoft.VisualStudio.R.Package.SurveyNews {
    [Export(typeof(ISurveyNewsFeedClient))]
    internal class SurveyNewsFeedClient : ISurveyNewsFeedClient {
        public Task<SurveyNewsFeed> GetFeedAsync(string feedUrl) {
            // We can't use a simple WebRequest, because that doesn't have access
            // to the browser's session cookies.  Cookies are used to remember
            // which survey/news item the user has submitted/accepted.  The server 
            // checks the cookies and returns the survey/news urls that are 
            // currently available (availability is determined via the survey/news 
            // item start and end date).
            var tcs = new TaskCompletionSource<SurveyNewsFeed>();
            try {
                var thread = new Thread(() => {
                    var browser = new WebBrowser();
                    browser.DocumentCompleted += (sender, e) => {
                        try {
                            if (browser.Url == e.Url) {
                                SurveyNewsFeed feed = ParseFeed(browser);
                                tcs.SetResult(feed);

                                Application.ExitThread();
                            }
                        } catch (Exception ex2) {
                            tcs.SetException(ex2);
                        }
                    };
                    browser.Navigate(new Uri(feedUrl));
                    Application.Run();
                });
                thread.Name = "SurveyNewsFeedClient";
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            } catch (Exception ex1) {
                tcs.SetException(ex1);
            }

            return tcs.Task;
        }

        private static SurveyNewsFeed ParseFeed(WebBrowser br) {
            // The survey/news server returns the data as content-type:text/plain
            // so the json is found in the document in a PRE element.
            string text = br.DocumentText;
            if (!string.IsNullOrEmpty(text)) {
                int startIndex = text.IndexOfIgnoreCase("<PRE>");
                if (startIndex > 0) {
                    int endIndex = text.IndexOfIgnoreCase("</PRE>", startIndex);
                    if (endIndex > 0) {
                        text = text.Substring(startIndex + 5, endIndex - startIndex - 5);
                        try {
                            return Json.DeserializeObject<SurveyNewsFeed>(text);
                        } catch (JsonReaderException ex) {
                            throw new SurveyNewsFeedException("Error deserializing json of survey/news feed.", ex);
                        }
                    }
                }
            }

            throw new SurveyNewsFeedException("Unable to parse survey/news feed.");
        }
    }
}
