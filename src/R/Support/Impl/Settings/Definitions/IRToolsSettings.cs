using Microsoft.Common.Core.Enums;

namespace Microsoft.R.Support.Settings.Definitions
{
    public interface IRToolsSettings
    {
        void LoadFromStorage();

        string RVersion { get; set; }

        string CranMirror { get; set; }

        YesNoAsk LoadRDataOnProjectLoad { get; set; }
        YesNoAsk SaveRDataOnProjectUnload { get; set; }

        bool EscInterruptsCalculation { get; set; }
    }
}
