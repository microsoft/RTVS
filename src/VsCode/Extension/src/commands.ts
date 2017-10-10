// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as vscode from "vscode";
import * as requests from "./rengine";
import * as editor from "./editor";
import { ReplTerminal } from "./repl";
import { ResultsServer } from "./resultsServer";
import REngine = requests.REngine;

// Must match package.json declarations
export namespace CommandNames {
    export const Execute = "r.execute";
    export const Interrupt = "r.interrupt";
    export const Reset = "r.reset";
    export const SourceFile = "r.source";
    export const Clear = "r.clear";
    export const OpenTerminal = "r.openTerminal";
    export const ExecuteInTerminal = "r.executeInTerminal";
    export const SourceFileToTerminal = "r.sourceToTerminal";
}

let r: REngine;
let repl: ReplTerminal;
let resultsServer: ResultsServer;

export class Commands {
    constructor(re: REngine, rt: ReplTerminal, rs: ResultsServer) {
        r = re;
        repl = rt;
        resultsServer = rs;
    }

    activateCommandsProvider(): vscode.Disposable[] {
        const disposables: vscode.Disposable[] = [];
        disposables.push(vscode.commands.registerCommand(CommandNames.Execute, () => this.execute()));
        disposables.push(vscode.commands.registerCommand(CommandNames.Interrupt, () => r.interrupt()));
        disposables.push(vscode.commands.registerCommand(CommandNames.Reset, () => r.reset()));
        disposables.push(vscode.commands.registerCommand(CommandNames.SourceFile, () => this.source()));
        disposables.push(vscode.commands.registerCommand(CommandNames.Clear, () => this.clear()));
        disposables.push(vscode.commands.registerCommand(CommandNames.OpenTerminal, () => repl.show()));
        disposables.push(vscode.commands.registerCommand(CommandNames.ExecuteInTerminal, () => this.executeInTerminal()));
        disposables.push(vscode.commands.registerCommand(CommandNames.SourceFileToTerminal, () => this.sourceToTerminal()));
        return disposables;
    }

    async source(fileUri?: vscode.Uri) {
        const filePath = editor.getFilePath(fileUri);
        if (filePath.length > 0) {
            await r.source(filePath);
        }
    }

    clear() {
        resultsServer.clearBuffer();
    }

    async execute() {
        const code = editor.getSelectedText();
        if (code.length > 0) {
            await r.execute(code);
        }
    }

    async sourceToTerminal(fileUri?: vscode.Uri) {
        const filePath = editor.getFilePath(fileUri);
        if (filePath.length > 0) {
            await this.sendTextToTerminal(`source("${filePath}")`);
        }
    }

    async executeInTerminal() {
        const code = editor.getSelectedText();
        if (code.length > 0) {
            await this. sendTextToTerminal(code);
            // Move caret down
            await vscode.commands.executeCommand("cursorMove",
                {
                    to: "down",
                    by: "line"
                });
        }
    }

    async sendTextToTerminal(text: string) {
        const repl = await this.getRepl();
        repl.sendText(text);
    }

    async getRepl() {
        if (repl !== undefined && repl != null) {
            return repl;
        }
        const interpreterPath = await r.getInterpreterPath();
        repl = new ReplTerminal(interpreterPath);
        repl.show();
        return repl;
    }
}