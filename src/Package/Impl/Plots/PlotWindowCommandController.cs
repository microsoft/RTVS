using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal sealed class PlotWindowCommandController : ICommandTarget {
        private PlotWindowMenu _menu;

        public PlotWindowCommandController(PlotWindowMenu menu) {
            _menu = menu;
        }

        public CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _menu.Execute(id);
            return CommandResult.Executed;
        }

        public void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
        }

        public CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }
    }
}
