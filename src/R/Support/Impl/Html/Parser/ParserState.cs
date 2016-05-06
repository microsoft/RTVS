// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Html.Core.Parser {
    public enum ParserState {
        Text,
        StartTag,
        Attributes,
        EndTag,
        Script,
        Style,
        Comment,
        Artifact
    }
}
