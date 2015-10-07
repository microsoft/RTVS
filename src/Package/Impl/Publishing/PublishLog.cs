using System;
using Microsoft.R.Actions.Logging;
using Microsoft.VisualStudio.R.Package.Logging;

namespace Microsoft.VisualStudio.R.Package.Publishing
{
    public sealed class PublishLog : OutputWindowLog
    {
        private static Guid _windowPaneGuid = new Guid("9E7E75B1-B120-4C32-86CC-3B0EB76D00C0");
        private static Lazy<PublishLog> _instance = new Lazy<PublishLog>(() => new PublishLog());

        public static IActionLog Current
        {
            get { return _instance.Value; }
        }

        private PublishLog() :
            base(_windowPaneGuid, Resources.OutputWindowName_Publish)
        {
        }
    }
}
