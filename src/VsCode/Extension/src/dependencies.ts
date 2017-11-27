// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as fs from "fs";
import { getenv } from "getenv";
import { opn } from "opn";
import * as vscode from "vscode";
import * as os from "./os";

export async function getR(r: IREngine): Promise<string> {
    const interpreterPath = await r.getInterpreterPath();
    if (interpreterPath === undefined || interpreterPath === null) {
        if (await vscode.window.showErrorMessage("Unable to find R interpreter. Would you like to install R now?",
                                                 "Yes", "No") === "Yes") {
            InstallR();
            vscode.window.showWarningMessage("Please restart VS Code after R installation is complete.");
        }
        return null;
    }
    return interpreterPath;
}

export async function checkDotNet(): Promise<boolean> {
    if (!IsDotNetInstalled()) {
        if (await vscode.window.showErrorMessage("R Tools require .NET Core Runtime. Would you like to install it now?",
                                                 "Yes", "No") === "Yes") {
            InstallDotNet();
            vscode.window.showWarningMessage("Please restart VS Code after .NET Runtime installation is complete.");
        }
        return false;
    }
    return true;
}

function IsDotNetInstalled() {
    const versions = ["1.1.2", "1.1.4", "2.0.0"];
    let prefix: string;

    if (os.IsWindows()) {
        const drive = getenv("SystemDrive");
        prefix = drive + "\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\";
    } else {
        prefix = "/usr/local/share/dotnet/shared/Microsoft.NETCore.App/";
    }

    for (const version of versions) {
        if (fs.existsSync(prefix + version)) {
            return true;
        }
    }
    return false;
}

function InstallDotNet() {
    opn("https://www.microsoft.com/net/download/core#/runtime");
}

function InstallR() {
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
