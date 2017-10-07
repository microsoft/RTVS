// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

"use strict";
import {client} from "./extension";

export async function getInterpreterPath(): Promise<string> {
    await client.onReady();
    return await client.sendRequest<string>("information/getInterpreterPath");
}

export async function getOutput(): Promise<string> {
    await client.onReady();
    return await client.sendRequest<string>("r/execute");
}