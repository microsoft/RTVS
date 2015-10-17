namespace Microsoft.R.Support.Settings.Definitions
{
    public interface IRToolsSettings
    {
        string RVersionPath { get; set; }

        string CranMirror { get; set; }
    }
}
