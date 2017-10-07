// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as os from "./os";
import * as fs from "fs";
import { getenv } from "getenv";
import { opn } from "opn";

export function IsDotNetInstalled() {
    const versions = ["1.1.2", "1.1.4", "2.0.0"];
    let prefix: string;

    if (os.IsWindows()) {
        const drive = getenv("SystemDrive");
        prefix = drive + "\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\";
    } else {
        prefix = "/usr/local/share/dotnet/shared/Microsoft.NETCore.App/";
    }

    for (let i = 0; i < versions.length; i++) {
        if (fs.existsSync(prefix + versions[i])) {
            return true;
        }
    }
    return false;
}

export function InstallDotNet() {
    let url: string;
    if (os.IsWindows()) {
        url = "https://download.microsoft.com/download/6/F/B/6FB4F9D2-699B-4A40-A674-B7FF41E0E4D2/dotnet-win-x64.1.1.4.exe";
    } else if (os.IsMac()) {
        url = "https://download.microsoft.com/download/6/F/B/6FB4F9D2-699B-4A40-A674-B7FF41E0E4D2/dotnet-osx-x64.1.1.4.pkg";
    } else {
        url = "https://www.microsoft.com/net/download/linux";
    }
    opn(url);
}

export function InstallR() {
    let url: string;
    if (os.IsWindows()) {
        url = "https://cran.r-project.org/bin/windows/base/";
    } else if (os.IsMac()) {
        url = "https://cran.r-project.org/bin/macosx/";
    } else {
        url = "https://cran.r-project.org/bin/linux/";
    }
    opn(url);
}