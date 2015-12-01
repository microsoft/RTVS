namespace Microsoft.R.Actions.Utility {
    public enum RInstallStatus {
        /// <summary>
        /// R is installed and compatible
        /// </summary>
        OK,

        /// <summary>
        /// No path to R can be found in Tools | Options
        /// </summary>
        Undefined,

        /// <summary>
        /// R is installed but version does not match
        /// </summary>
        UnsupportedVersion,

        /// <summary>
        /// Path appears to exist but no R.dll can be found
        /// </summary>
        NoRBinaries,

        /// <summary>
        /// Specified path to R binaries does not exist
        /// </summary>
        InvalidInstallPath
    }
}
