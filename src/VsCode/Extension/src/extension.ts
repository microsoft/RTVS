"use strict";
// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from "vscode";
import * as languageClient from "vscode-languageclient";
import * as path from "path";
import * as fs from "fs";

// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) {
    console.log("Activating R extension...");
    activateLanguageServer(context);
    console.log("R extension is now activated.");

    // The command has been defined in the package.json file
    // Now provide the implementation of the command with  registerCommand
    // The commandId parameter must match the command field in package.json
    const disposable = vscode.commands.registerCommand("extension.sayHello", () => {
        // The code you place here will be executed every time your command is executed

        // Display a message box to the user
        vscode.window.showInformationMessage("Hello World!");
    });

    context.subscriptions.push(disposable);
}

export function activateLanguageServer(context: vscode.ExtensionContext) {

    // The server is implemented in C#
    const commandOptions = { stdio: "pipe" };
    const serverModule = "Microsoft.R.LanguageServer.dll";

    // If the extension is launched in debug mode then the debug server options are used
    // Otherwise the run options are used
    const serverOptions: languageClient.ServerOptions = {
        run : { command: "dotnet", args: [serverModule], options: commandOptions },
        debug: { command: "dotnet", args: [serverModule, "--debug"], options: commandOptions }
    };
    
    // Options to control the language client
    const clientOptions: languageClient.LanguageClientOptions = {
        // Register the server for plain text documents
        documentSelector: ["r"],
        synchronize: {
            // Synchronize the setting section 'languageServerExample' to the server
            configurationSection: "RLanguageServer",
            // Notify the server about file changes to '.clientrc files contain in the workspace
            fileEvents: [
                vscode.workspace.createFileSystemWatcher("**/.clientrc"),
                vscode.workspace.createFileSystemWatcher("**/.demo")
            ]
        }
    };
    
    // Create the language client and start the client.
    let disposable = new languageClient.LanguageClient("R", "R Language Support", serverOptions, clientOptions).start();
    
    // Push the disposable to the context's subscriptions so that the 
    // client can be deactivated on extension deactivation
    context.subscriptions.push(disposable);
}

// this method is called when your extension is deactivated
export function deactivate() {
}