// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as vscode from "vscode";
import { Disposable } from "vscode";
import { RPReviewSchema } from "./constants";
import { createDeferred } from "./deferred";

const viewResultsUri = vscode.Uri.parse(RPReviewSchema + "://results");

export class ResultsView extends Disposable implements vscode.TextDocumentContentProvider, IResultsView {
    private _onDidChange = new vscode.EventEmitter<vscode.Uri>();
    private uri: vscode.Uri;
    private buffer: string = "";

    constructor() {
        super(() => { });
    }

    public dispose() { }

    public provideTextDocumentContent(uri: vscode.Uri, token: vscode.CancellationToken): Thenable<string> {
        this.uri = uri;
        return this.generateResultsView();
    }

    get onDidChange(): vscode.Event<vscode.Uri> {
        return this._onDidChange.event;
    }

    public clear() {
        this.updateBuffer("");
    }

    public async append(code: string, result: string) {
        this.openResultsView();

        if (code.length > 64) {
            code = code.substring(0, 256).concat("...");
        }
        code = `&gt; ${this.formatCode(code)}`;

        let breaksAfterCode = "<br/>";
        let breaksAfterOutput = "";

        let output: string;
        if (result.startsWith("$$IMAGE ")) {
            const base64 = result.substring(8, result.length);
            breaksAfterCode = breaksAfterCode + "<br/>";
            breaksAfterOutput = "<br/>";
            // tslint:disable-next-line:max-line-length
            output = `<img src='data:image/gif;base64, ${base64}' style='display:block; margin: 8,0,8,0; text-align: center; width: 90%' />`;
        } else if (result.startsWith("$$ERROR ")) {
            const error = result.substring(8, result.length);
            output = this.formatError(error);
        } else {
            if (result.length === 0) {
                output = "<span style='color: green'>[done]</span><br />";
            } else {
                output = this.formatOutput(result);
            }
        }

        this.updateBuffer(this.buffer.concat(code + breaksAfterCode + output + breaksAfterOutput));
    }

    private openResultsView() {
        const def = createDeferred<any>();

        vscode.commands.executeCommand("vscode.previewHtml", viewResultsUri, vscode.ViewColumn.Two, "Results")
            .then(() => {
                def.resolve();
            }, (reason) => {
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
        return `<span style='color: red'>${this.encodeHtml(text)}</span><br />`;
    }

    private formatOutput(code: string): string {
        return `<pre>${this.encodeHtml(code)}</pre>`;
    }

    private formatCode(code: string): string {
        return `<span style='${this.getTextStyle()}'>${this.encodeHtml(code)}</span>`;
    }

    private encodeHtml(html: string): string {
        return html.replace("<", "&lt;").replace(">", "&gt;");
    }

    private getTextStyle(): string {
        const editorConfig = vscode.workspace.getConfiguration("editor");
        const fontFamily = editorConfig.get<string>("fontFamily").split("'").join("").split('"').join("");
        const fontSize = editorConfig.get<number>("fontSize") + "px";
        const fontWeight = editorConfig.get<string>("fontWeight");
        return `fontFamily: ${fontFamily}; fontSize: ${fontSize}; fontWeight: ${fontWeight};`;
    }

    private updateBuffer(content: string) {
        this.buffer = content;
        this._onDidChange.fire(this.uri);

    }
}
