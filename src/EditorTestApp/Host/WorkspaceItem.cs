// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Workspace;

namespace Microsoft.Languager.Editor.Application
{
    [ExcludeFromCodeCoverage]
    public class WorkspaceItem : IWorkspaceItem
    {
        public WorkspaceItem(string moniker, string path)
        {
            Moniker = moniker;
            Path = path;
        }

        public string Moniker
        {
            get; private set;
        }

        public string Path
        {
           get; private set;
        }

        public void Dispose()
        {
        }

#pragma warning disable 67
        public event EventHandler<EventArgs> Changed;
    }
}
