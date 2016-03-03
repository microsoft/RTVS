// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Maps to code in RApi.h that specify returns
    /// from the YesNoCancel callback. Note that returns
    /// are different from Windows IDNO, IDANCEL, IDYES.
    /// </summary>
    public enum YesNoCancel {
        No = -1,
        Cancel = 0,
        Yes = 1
    }
}
