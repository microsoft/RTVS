namespace Microsoft.R.Engine.Settings.Definitions
{
    public interface IRToolsSettings
    {
        string GetRVersionPath();
        int HelpPortNumber { get; }
    }
}
