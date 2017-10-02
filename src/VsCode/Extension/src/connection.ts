// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

"use strict";
import {client} from "./extension";

export async function getInterpreterPath(): Promise<string> {
    await client.onReady();
    const path = await client.sendRequest<string>("information/getInterpreterPath");
    return path;
}