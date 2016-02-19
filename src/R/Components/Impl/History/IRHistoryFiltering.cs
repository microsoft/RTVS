namespace Microsoft.R.Components.History {
    public interface IRHistoryFiltering {
        void ClearFilter();
        void Filter(string searchPattern);
    }
}