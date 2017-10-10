// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as vscode from "vscode";

let terminal: vscode.Terminal;
let interpreterPath: string;

export class ReplTerminal {
 
    constructor(ip: string) {
        interpreterPath = ip;

        vscode.window.onDidCloseTerminal((closedTerminal: vscode.Terminal) => {
            if (terminal === closedTerminal) {
                terminal = undefined;
            }
        });
    }

    show() {
        if (terminal === undefined) {
            terminal = vscode.window.createTerminal("R", interpreterPath);
        }
        terminal.show();
    }

    close() {
        if (terminal !== undefined) {
            terminal.dispose();
            terminal = undefined;
        }
    }

    sendText(text: string) {
        this.show();
        terminal.sendText(text);
    }
}

