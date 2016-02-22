namespace Microsoft.R.Debugger {
    public enum DebugValueRepresentationKind {
        /// <summary>
        /// Converted for better representation in UI.
        /// For example, fancy quotes may be converted
        /// from 0x91-0x94 to Unicode equivalents.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Ar returned by R
        /// </summary>
        Raw = 1,
    }
}
