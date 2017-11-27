// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

interface IResultsServer {
    start(): Promise<number>;
    clearBuffer();
    send(data: string): Thenable<void>;
    dispose();
}
