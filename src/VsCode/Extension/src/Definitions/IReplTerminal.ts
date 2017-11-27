// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
"use strict";

interface IReplTerminal {
    show();
    close();
    sendText(text: string);
}
