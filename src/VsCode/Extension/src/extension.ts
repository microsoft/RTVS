// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from "vscode";
import * as languageClient from "vscode-languageclient";
import * as term from "./terminal";

// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export async function activate(context: vscode.ExtensionContext) {
    console.log("Activating R Tools...");
    await activateLanguageServer(context);
    console.log("R Tools is now activated.");
}

export async function activateLanguageServer(context: vscode.ExtensionContext) {

    // The server is implemented in C#
    const commandOptions = { stdio: "pipe" };
    const serverModule = context.extensionPath + "/server/Microsoft.R.LanguageServer.dll";

    // If the extension is launched in debug mode then the debug server options are used
    // Otherwise the run options are used
    const serverOptions: languageClient.ServerOptions = {
        run: { command: "dotnet", args: [serverModule, "--debug"], options: commandOptions },
        debug: { command: "dotnet", args: [serverModule, "--debug"], options: commandOptions }
    };

    // Options to control the language client
    const clientOptions: languageClient.LanguageClientOptions = {
        // Register the server for R documents
        documentSelector: ["r"],
        synchronize: {
            configurationSection: "r"
        }
    };

    // Create the language client and start the client.
    context.subscriptions.push(new languageClient.LanguageClient("r", "R Tools", serverOptions, clientOptions).start());
    context.subscriptions.push(...term.activateExecInTerminalProvider());

    await term.startRepl();
}

// this method is called when your extension is deactivated
export function deactivate() {
}