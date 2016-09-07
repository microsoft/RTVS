// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Microsoft.Common.Core;
using System.Threading;
using System.Collections.Specialized;
using System.Text;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class RemoteUriWebService : IRemoteUriWebService {
        public RemoteUriWebService(string baseUri) {
            PostUri = new Uri(new Uri(baseUri),"/remoteuri");
        }
        private Uri PostUri { get; }

        public async Task GetResponseAsync(HttpListenerContext context, string localBaseUrl, string remoteBaseUrl, CancellationToken ct) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PostUri);
            request.Method = context.Request.HttpMethod;
            SetRequestHeaders(request, context.Request.Headers, localBaseUrl, remoteBaseUrl);

            // Add RTVS headers
            request.Headers.Add("x-rtvs-url", GetRemoteUrl(context.Request.Url, remoteBaseUrl));

            if(context.Request.InputStream.CanSeek && context.Request.InputStream.Length > 0) {
                Stream reqStream = await request.GetRequestStreamAsync();
                await context.Request.InputStream.CopyToAsync(reqStream);
                await reqStream.FlushAsync();
            }
            
            HttpWebResponse response =  (HttpWebResponse)await request.GetResponseAsync();

            SetResponseHeaders(response, context.Response, localBaseUrl, remoteBaseUrl);
            Stream respStream = response.GetResponseStream();

            await respStream.CopyToAsync(context.Response.OutputStream);
            await context.Response.OutputStream.FlushAsync();
            context.Response.OutputStream.Close();
        }

        private string GetRemoteUrl(Uri url, string remoteBase) {
            Uri remote = new Uri($"http://{remoteBase}");
            UriBuilder ub = new UriBuilder(url);
            ub.Host = remote.Host;
            ub.Port = remote.Port;
            return ub.Uri.ToString();
        }

        private static long GetLong(string value) {
            long ret;
            if (long.TryParse(value, out ret)) {
                return ret;
            }
            return 0;
        }

        private static DateTime GetDateTime(string value) {
            DateTime ret;
            if (DateTime.TryParse(value, out ret)) {
                return ret;
            }
            return new DateTime();
        }

        private static string ReplaceAndGet(string value, string url1, string url2) {
            return value.Replace(url1, url2);
        }

        private static void SetRequestHeaders(HttpWebRequest request, NameValueCollection requestHeaders, string localBaseUrl, string remoteBaseUrl) {
            // copy headers to avoid messing with original request headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach(string key in requestHeaders.AllKeys) {
                headers.Add(key, requestHeaders[key]);
            }

            if (headers.ContainsKey("Accept")) {
                request.Accept = ReplaceAndGet(headers["Accept"], localBaseUrl, remoteBaseUrl);
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
                request.ContentLength = GetLong(headers["Content-Length"]);
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
                request.Date = GetDateTime(headers["Date"]);
                headers.Remove("Date");
            }

            if (headers.ContainsKey("Host")) {
                request.Host = ReplaceAndGet(headers["Host"], localBaseUrl, remoteBaseUrl);
                headers.Remove("Host");
            }

            if (headers.ContainsKey("If-Modified-Since")) {
                request.IfModifiedSince = GetDateTime(headers["If-Modified-Since"]);
                headers.Remove("If-Modified-Since");
            }

            if (headers.ContainsKey("Range")) {
                // TODO: AddRange
                headers.Remove("Range");
            }

            if (headers.ContainsKey("Referer")) {
                request.Referer = ReplaceAndGet(headers["Referer"], localBaseUrl, remoteBaseUrl);
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
                request.Headers.Add(pair.Key, ReplaceAndGet(pair.Value, localBaseUrl, remoteBaseUrl));
            }
        }

        private static void SetResponseHeaders(HttpWebResponse response, HttpListenerResponse httpListenerResponse, string localBaseUrl, string remoteBaseUrl) {
            // copy headers to avoid messing with original response headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (var key in response.Headers.AllKeys) {
                headers.Add(key, ReplaceAndGet(response.Headers[key], remoteBaseUrl, localBaseUrl));
            }

            httpListenerResponse.ContentLength64 = response.ContentLength;
            httpListenerResponse.ContentType = response.ContentType;

            if (!string.IsNullOrWhiteSpace(response.ContentEncoding)) {
                httpListenerResponse.ContentEncoding = Encoding.GetEncoding(response.ContentEncoding);
            }
            
            httpListenerResponse.StatusCode = (int)response.StatusCode;

            if (headers.ContainsKey("Content-Length")) {
                headers.Remove("Content-Length");
            }

            if (headers.ContainsKey("Content-Type")) {
                headers.Remove("Content-Type");
            }

            if (headers.ContainsKey("Content-Encoding")) {
                headers.Remove("Content-Encoding");
            }

            foreach (var pair in headers) {
                httpListenerResponse.Headers.Add(pair.Key, pair.Value);
            }
        }
    }
}
