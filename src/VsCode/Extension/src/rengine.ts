// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as languageClient from "vscode-languageclient";
import {ResultsServer} from "./resultsServer";

export class REngine {
     resultsServer: ResultsServer;
     client: languageClient.LanguageClient;

     constructor(client: languageClient.LanguageClient, resultsServer: ResultsServer) {
         this.client = client;
        this.resultsServer = resultsServer;
    }

    getInterpreterPath(): Thenable<string> {
        return this.client.sendRequest<string>("information/getInterpreterPath");
    }

    async execute(code: string) {
        const result = await this.client.sendRequest<string>("r/execute", code);
        this.resultsServer.sendResults(code, result);
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