// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import { EOL } from "os";
import * as vscode from "vscode";
import { RLanguage } from "./constants";

export function getFilePath(fileUri?: vscode.Uri): string {
    let filePath = "";

    if (fileUri === undefined || fileUri === null || typeof fileUri.fsPath !== "string") {
        const activeEditor = vscode.window.activeTextEditor;
        if (activeEditor !== undefined && activeEditor !== null) {
            if (!activeEditor.document.isUntitled) {
                if (activeEditor.document.languageId === RLanguage.language) {
                    filePath = activeEditor.document.fileName;
                } else {
                    vscode.window.showErrorMessage("The active file is not a R source file.");
                }
            } else {
                vscode.window.showErrorMessage("The active file needs to be saved before it can be sourced.");
            }
        } else {
            vscode.window.showErrorMessage("No open R file to source.");
        }
    } else {
        filePath = fileUri.fsPath;
    }

    if (filePath.indexOf(" ") > 0) {
        filePath = `"${filePath}"`;
    }

    return filePath;
}

export function getSelectedText(): string {
    const activeEditor = vscode.window.activeTextEditor;
    if (!activeEditor) {
        return "";
    }

    const selection = activeEditor.selection;
    let code: string;
    if (selection.isEmpty) {
        code = activeEditor.document.lineAt(selection.start.line).text;
    } else {
        const textRange = new vscode.Range(selection.start, selection.end);
        code = activeEditor.document.getText(textRange);
    }

    return removeBlankLines(code);
}

function removeBlankLines(code: string): string {
    const codeLines = code.split(/\r?\n/g);
    const codeLinesWithoutEmptyLines = codeLines.filter((line) => line.trim().length > 0);
    const lastLineIsEmpty = codeLines.length > 0 && codeLines[codeLines.length - 1].trim().length === 0;
    if (lastLineIsEmpty) {
        codeLinesWithoutEmptyLines.unshift("");
    }
    return codeLinesWithoutEmptyLines.join(EOL);
}
