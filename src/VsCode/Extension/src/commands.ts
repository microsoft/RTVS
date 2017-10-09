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

export class Commands {
    r: REngine;
    repl: ReplTerminal;
    resultsServer: ResultsServer;

    constructor(r: REngine, repl: ReplTerminal, resultsServer: ResultsServer) {
        this.r = r;
        this.repl = repl;
        this.resultsServer = resultsServer;
    }

    activateCommandsProvider(): vscode.Disposable[] {
        const disposables: vscode.Disposable[] = [];
        disposables.push(vscode.commands.registerCommand(CommandNames.Execute, this.r.execute));
        disposables.push(vscode.commands.registerCommand(CommandNames.Interrupt, () => this.r.interrupt()));
        disposables.push(vscode.commands.registerCommand(CommandNames.Reset, () => this.r.reset()));
        disposables.push(vscode.commands.registerCommand(CommandNames.SourceFile, this.r.source));
        disposables.push(vscode.commands.registerCommand(CommandNames.Clear, this.clear));
        disposables.push(vscode.commands.registerCommand(CommandNames.OpenTerminal, () => this.repl.show()));
        disposables.push(vscode.commands.registerCommand(CommandNames.ExecuteInTerminal, this.executeInTerminal));
        disposables.push(vscode.commands.registerCommand(CommandNames.SourceFileToTerminal, this.sourceToTerminal));
        return disposables;
    }

    async source(fileUri?: vscode.Uri) {
        const filePath = editor.getFilePath(fileUri);
        if (filePath.length > 0) {
            await this.r.source(filePath);
        }
    }

    clear() {
        this.resultsServer.clearBuffer();
    }

    async execute() {
        const code = editor.getSelectedText();
        if (code.length > 0) {
            await this.r.execute(code);
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
        if (this.repl !== undefined && this.repl != null) {
            return this.repl;
        }
        const interpreterPath = await this.r.getInterpreterPath();
        this.repl = new ReplTerminal(interpreterPath);
        this.repl.show();
        return this.repl;
    }
}