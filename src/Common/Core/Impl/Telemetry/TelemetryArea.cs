//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace Microsoft.Common.Core.Telemetry {
    /// <summary>
    /// Area names show up as part of telemetry event names like:
    ///   VS/RTools/[area]/[event]
    public enum TelemetryArea {
        // Keep these sorted
        Build,
        Configuration,
        Debugger,
        Editor,
        History,
        Options,
        Packages,
        Plotting,
        Project,
        Repl,
        VariableExplorer,
        DataGrid,
    }
}
