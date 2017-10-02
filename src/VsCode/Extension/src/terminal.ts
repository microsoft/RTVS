// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

"use strict";
import * as vscode from "vscode";
import { EOL } from "os";
import { Commands, RLanguage } from "./constants";
import { getInterpreterPath } from "./connection";

let terminal: vscode.Terminal;

export function activateExecInTerminalProvider(): vscode.Disposable[] {
    const disposables: vscode.Disposable[] = [];
    disposables.push(vscode.commands.registerCommand(Commands.StartRepl, startRepl));
    disposables.push(vscode.commands.registerCommand(Commands.SourceFile, sourceFile));
    disposables.push(vscode.commands.registerCommand(Commands.ExecInTerminal, execInTerminal));
    disposables.push(vscode.window.onDidCloseTerminal((closedTerminal: vscode.Terminal) => {
        if (terminal === closedTerminal) {
            terminal = null;
        }
    }));
    return disposables;
}

async function sourceFile(fileUri?: vscode.Uri) {
    let filePath: string;

    if (fileUri === undefined || fileUri === null || typeof fileUri.fsPath !== "string") {
        const activeEditor = vscode.window.activeTextEditor;
        if (activeEditor !== undefined) {
            if (!activeEditor.document.isUntitled) {
                if (activeEditor.document.languageId === RLanguage.language) {
                    filePath = activeEditor.document.fileName;
                } else {
                    vscode.window.showErrorMessage("The active file is not a R source file");
                    return;
                }
            } else {
                vscode.window.showErrorMessage("The active file needs to be saved before it can be run");
                return;
            }
        } else {
            vscode.window.showErrorMessage("No open R file to run in terminal");
            return;
        }
    } else {
        filePath = fileUri.fsPath;
    }

    if (filePath.indexOf(" ") > 0) {
        filePath = `"${filePath}"`;
    }

    await startRepl();
    terminal.sendText(`source("${filePath}")`);
}

async function execInTerminal() {
    const activeEditor = vscode.window.activeTextEditor;
    if (!activeEditor) {
        return;
    }

    const selection = activeEditor.selection;
    let code: string;
    if (selection.isEmpty) {
        code = activeEditor.document.lineAt(selection.start.line).text;
    }
    else {
        const textRange = new vscode.Range(selection.start, selection.end);
        code = activeEditor.document.getText(textRange);
    }

    if (code.length === 0) {
        return;
    }

    await startRepl();
    terminal.sendText(removeBlankLines(code));
    // Move caret down
    await vscode.commands.executeCommand("cursorMove", {
        to: "down",
        by: "line"
    });
}

export async function startRepl() {
    if (terminal === null || terminal === undefined) {
        terminal = await createTerminal();
        terminal.show();
    }
}

async function createTerminal(): Promise<vscode.Terminal> {
    // const hover = await vscode.commands.executeCommand("vscode.executeHoverProvider", 
    //     vscode.window.activeTextEditor.document.uri, vscode.window.activeTextEditor.selection.start);
    const interpreterPath = await getInterpreterPath();
    //const interpreterPath = "C:\\Program Files\\R\\R-3.4.0\\bin\\x64\\R.exe";
    return vscode.window.createTerminal("R", interpreterPath);
}

function removeBlankLines(code: string): string {
    const codeLines = code.split(/\r?\n/g);
    const codeLinesWithoutEmptyLines = codeLines.filter(line => line.trim().length > 0);
    const lastLineIsEmpty = codeLines.length > 0 && codeLines[codeLines.length - 1].trim().length === 0;
    if (lastLineIsEmpty) {
        codeLinesWithoutEmptyLines.unshift("");
    }
    return codeLinesWithoutEmptyLines.join(EOL);
}
