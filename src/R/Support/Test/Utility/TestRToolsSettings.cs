using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.R.Support.Test.Utility
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IRToolsSettings))]
    class TestRToolsSettings : IRToolsSettings
    {
        public string CranMirror
        {
            get { return string.Empty; }
            set { }
        }

        public string RVersion
        { 
            get { return "[Latest]"; }
            set { }
        }

        public void LoadFromStorage()
        {
        }
    }
}
