// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Common.Core;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.RemoteUri {
    public class RemoteUriHelper {
        public static async Task HandlerAsync(HttpContext context) {
            var url = context.Request.Headers[CustomHttpHeaders.RTVSRequestedURL];

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = context.Request.Method;
            SetRequestHeaders(request, context.Request.Headers);

            if (context.Request.ContentLength > 0) {
                using (Stream reqStream = await request.GetRequestStreamAsync()) {
                    await context.Request.Body.CopyToAsync(reqStream);
                    await reqStream.FlushAsync();
                }
            }

            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

            SetResponseHeaders(response, context.Response);

            using (Stream respStream = response.GetResponseStream()) {
                await respStream.CopyToAsync(context.Response.Body);
                await context.Response.Body.FlushAsync();
            }

            response.Close();
        }

        private static void SetRequestHeaders(HttpWebRequest request, IHeaderDictionary requestHeaders) {
            // copy headers to avoid messing with original request headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (var pair in requestHeaders) {
                headers.Add(pair.Key, string.Join(", ", pair.Value));
            }

            if (headers.ContainsKey("Accept")) {
                request.Accept = headers["Accept"];
                headers.Remove("Accept");
            }

            if (headers.ContainsKey("Connection")) {
                if (headers["Connection"].EqualsIgnoreCase("keep-alive")) {
                    request.KeepAlive = true;
                } else if (headers["Connection"].EqualsIgnoreCase("close")) {
                    request.KeepAlive = false;
                }
                headers.Remove("Connection");
            }

            if (headers.ContainsKey("Content-Length")) {
                request.ContentLength = headers["Content-Length"].ToLongOrDefault();
                headers.Remove("Content-Length");
            }

            if (headers.ContainsKey("Content-Type")) {
                request.ContentType = headers["Content-Type"];
                headers.Remove("Content-Type");
            }

            if (headers.ContainsKey("Expect")) {
                request.Expect = headers["Expect"];
                headers.Remove("Expect");
            }

            if (headers.ContainsKey("Date")) {
                request.Date = headers["Date"].ToDateTimeOrDefault();
                headers.Remove("Date");
            }

            if (headers.ContainsKey("Host")) {
                request.Host = headers["Host"];
                headers.Remove("Host");
            }

            if (headers.ContainsKey("If-Modified-Since")) {
                request.IfModifiedSince = headers["If-Modified-Since"].ToDateTimeOrDefault();
                headers.Remove("If-Modified-Since");
            }

            if (headers.ContainsKey("Range")) {
                // TODO: AddRange
                headers.Remove("Range");
            }

            if (headers.ContainsKey("Referer")) {
                request.Referer = headers["Referer"];
                headers.Remove("Referer");
            }

            if (headers.ContainsKey("Transfer-Encoding")) {
                request.SendChunked = true;
                request.TransferEncoding = headers["Transfer-Encoding"];
                headers.Remove("Transfer-Encoding");
            }

            if (headers.ContainsKey("User-Agent")) {
                request.UserAgent = headers["User-Agent"];
                headers.Remove("User-Agent");
            }

            foreach (var pair in headers) {
                request.Headers.Add(pair.Key, pair.Value);
            }
        }

        private static void SetResponseHeaders(HttpWebResponse response, HttpResponse httpResponse) {
            // copy headers to avoid messing with original response headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (var key in response.Headers.AllKeys) {
                headers.Add(key, response.Headers[key]);
            }

            httpResponse.ContentLength = response.ContentLength;
            httpResponse.ContentType = response.ContentType;
            httpResponse.StatusCode = (int)response.StatusCode;

            if (headers.ContainsKey("Content-Length")) {
                headers.Remove("Content-Length");
            }

            if (headers.ContainsKey("Content-Type")) {
                headers.Remove("Content-Type");
            }

            foreach (var pair in headers) {
                httpResponse.Headers.Add(pair.Key, pair.Value);
            }
        }
    }
}
