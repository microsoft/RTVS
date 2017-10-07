// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

export function IsWindows() {
    return process.platform === "win32";
}

export function IsMac() {
    return process.platform === "darwin";
}

export function IsLinux() {
    return process.platform === "linux";
}
