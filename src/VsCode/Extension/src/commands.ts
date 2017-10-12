// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as vscode from "vscode";
import * as editor from "./editor";
import { ReplTerminal } from "./replTerminal";

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
    private r: IREngine;
    private repl: IReplTerminal;
    private resultsView: IResultsView;

    constructor(r: IREngine, resultsView: IResultsView) {
        this.r = r;
        this.resultsView = resultsView;
    }

    activateCommandsProvider(): vscode.Disposable[] {
        const disposables: vscode.Disposable[] = [];
        disposables.push(vscode.commands.registerCommand(CommandNames.Execute, () => this.execute()));
        disposables.push(vscode.commands.registerCommand(CommandNames.Interrupt, () => this.r.interrupt()));
        disposables.push(vscode.commands.registerCommand(CommandNames.Reset, () => this.r.reset()));
        disposables.push(vscode.commands.registerCommand(CommandNames.SourceFile, () => this.source()));
        disposables.push(vscode.commands.registerCommand(CommandNames.Clear, () => this.clear()));
        disposables.push(vscode.commands.registerCommand(CommandNames.OpenTerminal, () => this.openTerminal()));
        disposables.push(vscode.commands.registerCommand(CommandNames.ExecuteInTerminal, () => this.executeInTerminal()));
        disposables.push(vscode.commands.registerCommand(CommandNames.SourceFileToTerminal, () => this.sourceToTerminal()));
        return disposables;
    }

    async source(fileUri?: vscode.Uri) {
        const filePath = editor.getFilePath(fileUri);
        if (filePath.length > 0) {
            await this.r.source(filePath);
        }
    }

    clear() {
        this.resultsView.clear();
    }

    async execute() {
        const code = editor.getSelectedText();
        if (code.length > 0) {
            const result = await this.r.execute(code);
            await this.resultsView.append(code, result);
            await this.moveCaretDown();
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
            await this.sendTextToTerminal(code);
            await this.moveCaretDown();
        }
    }

    async sendTextToTerminal(text: string) {
        const repl = await this.getRepl();
        repl.sendText(text);
    }

    async openTerminal() {
        await this.createTerminal();
        this.repl.show();
    }

    private async getRepl() {
        await this.createTerminal();
        this.repl.show();
        return this.repl;
    }

    private async createTerminal() {
        if (this.repl !== undefined && this.repl !== null) {
            return;
        }
        const interpreterPath = await this.r.getInterpreterPath();
        this.repl = new ReplTerminal(interpreterPath);
    }

    private async moveCaretDown() {
        const selectionEmpty = vscode.window.activeTextEditor.selection.isEmpty;
        if (selectionEmpty) {
            await vscode.commands.executeCommand("cursorMove",
                {
                    to: "down",
                    by: "line"
                });
        }
    }
}