using System;
using System.Diagnostics;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal sealed class PlotWindowCommandController : ICommandTarget {
        private PlotWindowPane _pane;

        public PlotWindowCommandController(PlotWindowPane pane) {
            Debug.Assert(pane != null);
            _pane = pane;
        }

        public CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            RPlotWindowContainer container = _pane.GetIVsWindowPane() as RPlotWindowContainer;
            Debug.Assert(container != null);
            container.Menu.Execute(id);
            return CommandResult.Executed;
        }

        public void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
        }

        public CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }
    }
}
