// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as vscode from "vscode";

export class ReplTerminal implements IReplTerminal {
    private terminal: vscode.Terminal;
    private interpreterPath: string;

    constructor(ip: string) {
        this.interpreterPath = ip;

        vscode.window.onDidCloseTerminal((closedTerminal: vscode.Terminal) => {
            if (this.terminal === closedTerminal) {
                this.terminal = undefined;
            }
        });
    }

    public show() {
        if (this.terminal === undefined) {
            this.terminal = vscode.window.createTerminal("R", this.interpreterPath);
        }
        this.terminal.show();
    }

    public close() {
        if (this.terminal !== undefined) {
            this.terminal.dispose();
            this.terminal = undefined;
        }
    }

    public sendText(text: string) {
        this.show();
        this.terminal.sendText(text);
    }
}
