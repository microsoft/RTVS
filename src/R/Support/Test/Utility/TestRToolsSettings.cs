using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Enums;
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


        public YesNoAsk LoadRDataOnProjectLoad {
            get { return YesNoAsk.Yes; }
            set { }
        }

        public YesNoAsk SaveRDataOnProjectUnload {
            get { return YesNoAsk.Yes; }
            set { }
        }
        
        public void LoadFromStorage()
        {
        }
    }
}
