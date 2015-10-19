using System.ComponentModel.Composition.Hosting;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.R.Support.Settings
{
    public static class RToolsSettings
    {
        private static object _lock = new object();

        private static IRToolsSettings _instance;
        private static ExportProvider _exportProvider;

        public static IRToolsSettings Current
        {
            get
            {
                lock(_lock)
                {
                    if (_instance == null)
                    {
                        _instance = _exportProvider != null ? _exportProvider.GetExport<IRToolsSettings>().Value : null;
                        _instance.LoadFromStorage();
                    }
                }

                return _instance;
            }
            internal set
            {
                _instance = value;
            }
        }

        public static void Init(ExportProvider exportProvider)
        {
            _exportProvider = exportProvider;
        }
    }
}
