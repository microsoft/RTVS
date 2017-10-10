// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from "vscode";
import * as languageClient from "vscode-languageclient";
import * as deps from "./dependencies";
import { RLanguage } from "./constants";
import { ResultsServer } from "./resultsServer";
import { REngine } from "./rengine";
import { Commands } from "./commands";
import { OutputPanel } from "./outputPanel";
import {ReplTerminal} from "./replTerminal";

let client: languageClient.LanguageClient;
let repl: IReplTerminal;
let rEngine: IREngine;
let commands: Commands;

// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export async function activate(context: vscode.ExtensionContext) {
    if (!deps.checkDotNet()) {
        return;
    }

    console.log("Activating R Tools...");
    await activateLanguageServer(context);
    console.log("R Tools is now activated.");
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

    await client.onReady();

    const resultsServer = new ResultsServer();
    context.subscriptions.push(resultsServer);

    rEngine = new REngine(client);
    const settings = vscode.workspace.getConfiguration(RLanguage.language);

    const interpreterPath = await deps.getR(rEngine);
    if (interpreterPath != null && settings.get<boolean>("r.useTerminal")) {
        repl = new ReplTerminal(interpreterPath);
    }

    commands = new Commands(rEngine, repl, resultsServer);
    context.subscriptions.push(...commands.activateCommandsProvider());

    const outputPanel = new OutputPanel();
    context.subscriptions.push(vscode.workspace.registerTextDocumentContentProvider("r", outputPanel));
}

// this method is called when your extension is deactivated
export async function deactivate() {
    if (client !== undefined || client !== null) {
        return client.stop();
    }
}
