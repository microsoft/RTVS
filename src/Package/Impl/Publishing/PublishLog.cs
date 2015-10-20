using System;
using Microsoft.R.Actions.Logging;
using Microsoft.VisualStudio.R.Package.Logging;

namespace Microsoft.VisualStudio.R.Package.Publishing {
    public sealed class PublishLog : LinesLog {
        private static readonly Guid WindowPaneGuid = new Guid("9E7E75B1-B120-4C32-86CC-3B0EB76D00C0");
        private static readonly Lazy<PublishLog> Instance = new Lazy<PublishLog>(() => new PublishLog());

        public static IActionLog Current => Instance.Value;

        private PublishLog() :
            base(new OutputWindowLogWriter(WindowPaneGuid, Resources.OutputWindowName_Publish)) {
        }
    }
}
