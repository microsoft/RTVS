// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Core.Parser {
    public interface IExpressionTermFilter {
        bool IsInertRange(ITextRange range);
    }
}
