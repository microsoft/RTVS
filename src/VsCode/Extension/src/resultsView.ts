// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as vscode from "vscode";
import { Disposable } from "vscode";
import {ResultsServer} from "./resultsServer";
import { createDeferred } from "./deferred";
import {RPReviewSchema} from "./constants";

const viewResultsUri = vscode.Uri.parse(RPReviewSchema + "://results");

export class ResultsView extends Disposable implements vscode.TextDocumentContentProvider, IResultsView {
    private _onDidChange = new vscode.EventEmitter<vscode.Uri>();
    private lastUri: vscode.Uri;
    private serverPort: number;
    private server: IResultsServer;

    constructor(extensionPath: string) {
        super(() => { });
        this.server = new ResultsServer(extensionPath);
    }

    dispose() {
        this.server.dispose();
    }

    set ServerPort(value: number) {
        this.serverPort = value;
    }

    provideTextDocumentContent(uri: vscode.Uri, token: vscode.CancellationToken): Thenable<string> {
        this.lastUri = uri;
        return this.generateResultsView();
    }

    get onDidChange(): vscode.Event<vscode.Uri> {
        return this._onDidChange.event;
    }

    clear() {
        this.server.clearBuffer();
    }
    
    async append(code: string, result: string) {
        await this.ensureServerStarted();

        if (code.length > 64) {
            code = code.substring(0, 64).concat("...");
        }
        code = `&gt; ${this.formatCode(code, null)}`;

        let output: string;
        if (result.startsWith("$$IMAGE ")) {
            const base64 = result.substring(8, result.length - 8);
            output = `<img src='data:image/gif;base64, ${base64}' style='display:block; margin: 8,0,8,0; text-align: center; width: 90%' />`;
        } else if (result.startsWith("$$ERROR ")) {
            const error = result.substring(8, result.length - 8);
            output = this.formatError(error);
        } else {
            output = this.formatCode(result, null);
        }

        this.server.send(code + "<br/>" + output);
    }

    private async ensureServerStarted() {
        this.serverPort = await this.server.start();
        this.openResultsView();
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
        // Fix for issue #669 "Results Panel not Refreshing Automatically" - always include a unique time
        // so that the content returned is different. Otherwise VSCode will not refresh the document since it
        // thinks that there is nothing to be updated.
        const timeNow = new Date().getTime();
        const htmlContent = `
                <!DOCTYPE html>
                <head>
                    <style type="text/css">
                        html, body{ height:100%; width:100%; } 
                    </style>
                    <script type="text/javascript">
                        function start(){
                            console.log('reloaded results window at time ${timeNow}ms');
                            var color = '';
                            var fontFamily = '';
                            var fontSize = '';
                            var theme = '';
                            var fontWeight = '';
                            try {
                                computedStyle = window.getComputedStyle(document.body);
                                color = computedStyle.color + '';
                                backgroundColor = computedStyle.backgroundColor + '';
                                fontFamily = computedStyle.fontFamily;
                                fontSize = computedStyle.fontSize;
                                fontWeight = computedStyle.fontWeight;
                                theme = document.body.className;
                            }
                            catch(ex){
                            }
                            document.getElementById('myframe').src =
                                'http://localhost:${this.serverPort}/?theme=' + theme + 
                                '&color=' + encodeURIComponent(color) + 
                                '&backgroundColor=' + encodeURIComponent(backgroundColor) + 
                                '&fontFamily=' + encodeURIComponent(fontFamily) + 
                                '&fontWeight=' + encodeURIComponent(fontWeight) + 
                                '&fontSize=' + encodeURIComponent(fontSize);
                        }
                    </script>
                </head>
                <body onload="start()">
                    <iframe id="myframe"
                            frameborder="0" 
                            style="border: 0px solid transparent; height:100%; width:100%;"
                            src="" 
                            seamless>
                    </iframe>
                </body>
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
        return `<code style='white-space: pre-wrap; display: block; ${style}'>${code}</code>`;
    }
}