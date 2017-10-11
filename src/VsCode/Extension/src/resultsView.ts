// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as vscode from "vscode";
import { Disposable } from "vscode";
import { createDeferred } from "./deferred";
import { RPReviewSchema } from "./constants";

const viewResultsUri = vscode.Uri.parse(RPReviewSchema + "://results");

export class ResultsView extends Disposable implements vscode.TextDocumentContentProvider, IResultsView {
    private _onDidChange = new vscode.EventEmitter<vscode.Uri>();
    private uri: vscode.Uri;
    private buffer: string = "";

    constructor(extensionPath: string) {
        super(() => { });
    }

    dispose() {
    }

    provideTextDocumentContent(uri: vscode.Uri, token: vscode.CancellationToken): Thenable<string> {
        this.uri = uri;
        return this.generateResultsView();
    }

    get onDidChange(): vscode.Event<vscode.Uri> {
        return this._onDidChange.event;
    }

    clear() {
        this.buffer = "";
    }

    async append(code: string, result: string) {
        this.openResultsView();

        if (code.length > 64) {
            code = code.substring(0, 64).concat("...");
        }
        code = `&gt; ${this.formatCode(code, null)}`;

        let breaksAfterCode = "<br/>";
        let breaksAfterOutput = "";

        let output: string;
        if (result.startsWith("$$IMAGE ")) {
            const base64 = result.substring(8, result.length - 8);
            breaksAfterCode = breaksAfterCode + "<br/>";
            breaksAfterOutput = "<br/>";
            output = `<img src='data:image/gif;base64, ${base64}' style='display:block; margin: 8,0,8,0; text-align: center; width: 90%' />`;
        } else if (result.startsWith("$$ERROR ")) {
            const error = result.substring(8, result.length - 8);
            output = this.formatError(error);
        } else {
            output = this.formatCode(result, null);
        }

        this.buffer = this.buffer.concat(code + breaksAfterCode + output + breaksAfterOutput);
        this._onDidChange.fire(this.uri);
    }

    private openResultsView() {
        const def = createDeferred<any>();

        vscode.commands.executeCommand("vscode.previewHtml", viewResultsUri, vscode.ViewColumn.Two, "Results")
            .then(() => {
                def.resolve();
            }, reason => {
                def.reject(reason);
                vscode.window.showErrorMessage(reason);
            });

        return def.promise;
    }

    private generateResultsView(): Promise<string> {
        const htmlContent = `
                <!DOCTYPE html>
                <head>
                    <style type="text/css">
                        html, body{ height:100%; width:100%; } 
                    </style>
                    <script type="text/javascript">
                        function start() {
                            window.scrollTo(0, document.body.scrollHeight)
                        }
                    </script>
                </head>
                <body onload='start()'>${this.buffer}</body>
            </html>`;

        return Promise.resolve(htmlContent);
    }

    private formatError(text: string): string {
        return this.formatCode(text, "color: red;");
    }

    private formatCode(code: string, style: string): string {
        if (style === undefined || style === null) {
            style = "";
        }
        return `<span style='${style}'>${code}</span>`;
    }
}