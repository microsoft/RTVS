// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as fs from "fs";
import * as opn from "opn";

export function IsWindows() {
    return process.platform === "win32";
}

export function IsMac() {
    return process.platform === "darwin";
}

export function IsLinux() {
    return process.platform === "linux";
}

export function IsDotNetInstalled() {
    if (IsWindows()) {
        return fs.existsSync("C:\\Program Files\\dotnet\\sdk");
    }
    return fs.existsSync("/usr/local/share/dotnet/shared/Microsoft.NETCore.App");
}

export function InstallDotNet() {
    opn("https://www.microsoft.com/net/core");
}