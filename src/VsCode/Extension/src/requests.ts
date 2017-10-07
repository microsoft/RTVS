// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

"use strict";
import {client} from "./extension";

export async function getInterpreterPath(): Promise<string> {
    await client.onReady();
    return await client.sendRequest<string>("information/getInterpreterPath");
}

export async function execute(code: string): Promise<string> {
    await client.onReady();
    return await client.sendRequest<string>("r/execute", code);
}

export async function interrupt() {
    await client.onReady();
    await client.sendRequest("r/interrupt");
}

export async function reset() {
    await client.onReady();
    await client.sendRequest("r/reset");
}

export async function source(filePath: string) {
    await client.onReady();
    await client.sendRequest("r/source", filePath);
}