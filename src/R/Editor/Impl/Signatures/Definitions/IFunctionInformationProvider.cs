// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Editor.Signatures.Definitions {
    /// <summary>
    /// An interface implemented by a function information provider that 
    /// serves function signature information to the completion engine. 
    /// There may be more than one provider. Providers are exported via MEF.
    /// </summary>
    public interface IFunctionInformationProvider {
        /// <summary>
        /// Retrieves function signature information
        /// </summary>
        /// <returns>
        /// Function information or null if fetching is asynchronous. In the latter case when 
        /// the information is ready, provider will call <paramref name="infoReadyCallback"/>
        /// passing <paramref name="parameter"/> as an argument. Callback can then re-trigger
        /// the signature session.
        /// </returns>
        IFunctionInfo GetFunctionInfo(RSignatureHelpContext context, string functionName, Action<object> infoReadyCallback = null, object parameter = null);
    }
}
