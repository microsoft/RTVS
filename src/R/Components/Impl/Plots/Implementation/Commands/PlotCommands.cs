// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal class PlotCommands : IRPlotCommands {
        public PlotCommands(IRInteractiveWorkflow interactiveWorkflow) {
            CopyAsBitmap = new CopyPlotAsBitmapCommand(interactiveWorkflow);
            CopyAsMetafile = new CopyPlotAsMetafileCommand(interactiveWorkflow);
            EndLocator = new EndLocatorCommand(interactiveWorkflow);
            ExportAsImage = new ExportPlotAsImageCommand(interactiveWorkflow);
            ExportAsPdf = new ExportPlotAsPdfCommand(interactiveWorkflow);
            Next = new NextPlotCommand(interactiveWorkflow);
            Previous = new PreviousPlotCommand(interactiveWorkflow);
            RemoveAll = new RemoveAllPlotsCommand(interactiveWorkflow);
            RemoveCurrent = new RemoveCurrentPlotCommand(interactiveWorkflow);
        }

        public IAsyncCommand CopyAsBitmap { get; }

        public IAsyncCommand CopyAsMetafile { get; }

        public IAsyncCommand EndLocator { get; }

        public IAsyncCommand ExportAsImage { get; }

        public IAsyncCommand ExportAsPdf { get; }

        public IAsyncCommand Next { get; }

        public IAsyncCommand Previous { get; }

        public IAsyncCommand RemoveAll { get; }

        public IAsyncCommand RemoveCurrent { get; }
    }
}
