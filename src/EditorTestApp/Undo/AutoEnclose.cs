// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.VisualStudio.Text.Operations.Standalone
{
    internal delegate void AutoEncloseDelegate();

    [ExcludeFromCodeCoverage]
    internal class AutoEnclose : IDisposable 
    {
        private AutoEncloseDelegate end;

        public AutoEnclose(AutoEncloseDelegate end)
        {
            this.end = end;
        }

        public void Dispose()
        {
            if (end != null)
            {
                end();
            }

            GC.SuppressFinalize(this);
        }
    }
}
