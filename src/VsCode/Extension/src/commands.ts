// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as vscode from "vscode";
import { repl } from "./extension";
import * as requests from "./requests";
import * as editor from "./editor";

// Must match package.json declarations
export namespace Commands {
    export const Execute = "r.execute";
    export const Interrupt = "r.interrupt";
    export const Reset = "r.reset";
    export const SourceFile = "r.source";
    export const Clear = "r.clear";
    export const OpenTerminal = "r.openTerminal";
    export const ExecuteInTerminal = "r.executeInTerminal";
    export const SourceFileToTerminal = "r.sourceToTerminal";
}

export function activateCommandsProvider(): vscode.Disposable[] {
    const disposables: vscode.Disposable[] = [];
    disposables.push(vscode.commands.registerCommand(Commands.Execute, execute));
    disposables.push(vscode.commands.registerCommand(Commands.Interrupt, () => requests.interrupt()));
    disposables.push(vscode.commands.registerCommand(Commands.Reset, () => requests.reset()));
    disposables.push(vscode.commands.registerCommand(Commands.SourceFile, source));
    disposables.push(vscode.commands.registerCommand(Commands.Clear, clear));
    disposables.push(vscode.commands.registerCommand(Commands.OpenTerminal, () => repl.show()));
    disposables.push(vscode.commands.registerCommand(Commands.ExecuteInTerminal, executeInTerminal));
    disposables.push(vscode.commands.registerCommand(Commands.SourceFileToTerminal, sourceToTerminal));
    return disposables;
}

async function source(fileUri?: vscode.Uri) {
    const filePath = editor.getFilePath(fileUri);
    if (filePath.length > 0) {
        await requests.source(filePath);
    }
}

async function clear() {
}

async function execute() {
    const code = editor.getSelectedText();
    if (code.length > 0) {
        const result = await requests.execute(code);
    }
}

function sourceToTerminal(fileUri?: vscode.Uri) {
    const filePath = editor.getFilePath(fileUri);
    if (filePath.length > 0) {
        repl.sendText(`source("${filePath}")`);
    }
}

async function executeInTerminal() {
    const code = editor.getSelectedText();
    if (code.length > 0) {
        repl.sendText(code);
        // Move caret down
        await vscode.commands.executeCommand("cursorMove", {
            to: "down",
            by: "line"
        });
    }
}



