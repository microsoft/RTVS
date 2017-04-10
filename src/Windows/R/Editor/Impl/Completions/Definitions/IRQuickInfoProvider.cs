// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// Get something (a string or WPF element) to show in a tooltip
    /// </summary>
    public interface IRQuickInfoProvider {
        object GetQuickInfo(int position);
    }
}
