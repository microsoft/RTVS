// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as languageClient from "vscode-languageclient";

export class REngine implements IREngine {
    private client: languageClient.LanguageClient;

    constructor(client: languageClient.LanguageClient) {
        this.client = client;
    }

    getInterpreterPath(): Thenable<string> {
        return this.client.sendRequest<string>("information/getInterpreterPath");
    }

    execute(code: string): Thenable<string> {
        return this.client.sendRequest<string>("r/execute", { "code": code });
    }

    async interrupt() {
        await this.client.sendRequest("r/interrupt");
    }

    async reset() {
        await this.client.sendRequest("r/reset");
    }

    async source(filePath: string) {
        await this.client.sendRequest("r/source", filePath);
    }
}