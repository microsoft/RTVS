// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.Help {
    /// <summary>
    /// Represents service that can crease CSS stylesheet for R Help vignettes
    /// from 
    /// </summary>
    public interface IVignetteCodeColorBuilder {
        string GetCodeColorsCss();
    }
}
