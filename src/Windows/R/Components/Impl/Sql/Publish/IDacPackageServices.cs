// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Sql.Publish {
    public interface IDacPackageServices {
        IDacPacBuilder GetBuilder();
        IDacPackage Load(string dacpacPath);
    }
}
