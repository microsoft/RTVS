// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Net;
using Microsoft.Common.Core.Services;

namespace Microsoft.R.Platform.Interpreters {
    /// <summary>
    /// Provides a way to download installer and launch setup 
    /// of the Microsoft R Client. Exported via MEF.
    /// </summary>
    public interface IMicrosoftRClientInstaller {
        void LaunchRClientSetup(IServiceContainer services, IFileDownloader downloader = null);
    }
}
