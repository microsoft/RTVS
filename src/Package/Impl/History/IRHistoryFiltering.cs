namespace Microsoft.VisualStudio.R.Package.History {
    public interface IRHistoryFiltering {
        void ClearFilter();
        void Filter(string searchPattern);
    }
}