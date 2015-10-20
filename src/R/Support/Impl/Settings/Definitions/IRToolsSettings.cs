namespace Microsoft.R.Support.Settings.Definitions
{
    public interface IRToolsSettings
    {
        void LoadFromStorage();

        string RVersion { get; set; }

        string CranMirror { get; set; }
    }
}
