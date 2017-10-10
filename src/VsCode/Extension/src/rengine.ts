// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

import * as languageClient from "vscode-languageclient";
import { ResultsServer } from "./resultsServer";

let resultsServer: ResultsServer;
let client: languageClient.LanguageClient;

export class REngine {
    constructor(c: languageClient.LanguageClient, rs: ResultsServer) {
        client = c;
        resultsServer = rs;
    }

    getInterpreterPath(): Thenable<string> {
        return client.sendRequest<string>("information/getInterpreterPath");
    }

    async execute(code: string) {
        const result = await client.sendRequest<string>("r/execute", { "code": code });
        resultsServer.sendResults(code, result);
    }

    async interrupt() {
        await client.sendRequest("r/interrupt");
    }

    async reset() {
        await client.sendRequest("r/reset");
    }

    async source(filePath: string) {
        await client.sendRequest("r/source", filePath);
    }
}