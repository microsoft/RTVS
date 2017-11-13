// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    [Guid(WindowGuidString)]
    internal class ConnectionManagerToolWindow : ViewContainerToolWindow {
        public const string WindowGuidString = "75753398-BE0E-442E-900C-E775EAC1FAC2";
        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);
        
        public ConnectionManagerToolWindow() { 
            BitmapImageMoniker = KnownMonikers.ImmediateWindow;
            Caption = Resources.WorkspacesWindowCaption;
        }
    }

    [Guid(WindowGuidString)]
    internal class ContainerManagerToolWindow : ViewContainerToolWindow {
        public const string WindowGuidString = "21B29559-2F66-4F41-B7B1-0378FD858DD3";
        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);
        
        public ContainerManagerToolWindow() { 
            BitmapImageMoniker = KnownMonikers.StorageContainer;
            Caption = Resources.ContainersWindowCaption;
        }
    }

    [Guid(WindowGuidString)]
    internal class PackageManagerToolWindow : ViewContainerToolWindow {
        public const string WindowGuidString = "363F84AD-3397-4FDE-97EA-1ABD73C64BB3";
        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);

        public PackageManagerToolWindow() {
            BitmapImageMoniker = KnownMonikers.Package;
            Caption = Resources.PackageManagerWindowCaption;
        }
    }
}
