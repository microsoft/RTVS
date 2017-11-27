// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

interface IREngine {
    getInterpreterPath(): Thenable<string>;
    execute(code: string);
    interrupt();
    reset();
    source(filePath: string);
}
