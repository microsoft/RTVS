// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from "vscode";
import * as languageClient from "vscode-languageclient";
import { RLanguage } from "./constants";
import { getInterpreterPath } from "./requests";
import * as utils from "./utils";
import {ReplTerminal} from "./repl";
import { activateCommandsProvider } from "./commands";

export let client: languageClient.LanguageClient;
export let repl: ReplTerminal;

// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export async function activate(context: vscode.ExtensionContext) {
    if (!checkDotNet()) {
        return;
    }

    console.log("Activating R Tools...");
    await activateLanguageServer(context);
    console.log("R Tools is now activated.");

    const interpreterPath = await getR();
    if (interpreterPath != null) {
        repl = new ReplTerminal(interpreterPath);
    }
}

export async function activateLanguageServer(context: vscode.ExtensionContext) {
    const r = RLanguage.language;
    // The server is implemented in C#
    const commandOptions = { stdio: "pipe" };
    const serverModule = context.extensionPath + "/server/Microsoft.R.LanguageServer.dll";

    // If the extension is launched in debug mode then the debug server options are used
    // Otherwise the run options are used
    const serverOptions: languageClient.ServerOptions = {
        run: { command: "dotnet", args: [serverModule], options: commandOptions },
        debug: { command: "dotnet", args: [serverModule, "--debug"], options: commandOptions }
    };

    // Options to control the language client
    const clientOptions: languageClient.LanguageClientOptions = {
        // Register the server for R documents
        documentSelector: [r],
        synchronize: {
            configurationSection: r
        }
    };

    // Create the language client and start the client.
    client = new languageClient.LanguageClient(r, "R Tools", serverOptions, clientOptions);
    context.subscriptions.push(client.start());
    context.subscriptions.push(...activateCommandsProvider());

    return client.onReady();
}

// this method is called when your extension is deactivated
export async function deactivate() {
    if (client !== undefined || client !== null) {
        return client.stop();
    }
}

async function getR(): Promise<string> {
    const interpreterPath = await getInterpreterPath();
    if (interpreterPath === undefined || interpreterPath === null) {
        if (await vscode.window.showErrorMessage("Unable to find R interpreter. Would you like to install R now?", "Yes", "No") === "Yes") {
            utils.InstallR();
            vscode.window.showWarningMessage("Please restart VS Code after R installation is complete.")
        }
        return null;
    }
    return interpreterPath;
}

async function checkDotNet(): Promise<boolean> {
    if (!utils.IsDotNetInstalled()) {
        if (await vscode.window.showErrorMessage("R Tools require .NET Core Runtime. Would you like to install it now?", "Yes", "No") === "Yes") {
            utils.InstallDotNet();
            vscode.window.showWarningMessage("Please restart VS Code after .NET Runtime installation is complete.")
        }
        return false;
    }
    return true;
}

async function checkDependencies() {
    if (!utils.IsDotNetInstalled()) {
        if (await vscode.window.showErrorMessage("R Tools require .NET Core Runtime. Would you like to install it now?", "Yes", "No") === "Yes") {
            utils.InstallDotNet();
            vscode.window.showWarningMessage("Please restart VS Code after .NET Runtime installation is complete.")
        }
        return false;
    }
    return true;
}    
