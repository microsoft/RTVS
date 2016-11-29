// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Broker.About {
    public class InfoHelper {
        public static async Task HandleAboutAsync(HttpContext context) {
            string about = JsonConvert.SerializeObject(AboutInfo.GetAboutHost());
            var bytes = Encoding.UTF8.GetBytes(about);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            await context.Response.Body.FlushAsync();
        }

        public static async Task HandleLoadAsync(HttpContext context) {
            string load = JsonConvert.SerializeObject(LoadInfo.GetLoad());
            var bytes = Encoding.UTF8.GetBytes(load);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            await context.Response.Body.FlushAsync();
        }
    }
}
