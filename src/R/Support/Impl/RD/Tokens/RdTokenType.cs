
namespace Microsoft.R.Support.RD.Tokens {
    // https://developer.r-project.org/parseRd.pdf

    public enum RdTokenType {
        /// <summary>
        /// Unrecognized token
        /// </summary>
        Unknown,

        /// <summary>
        /// % comment, lasts to the end of the line
        /// </summary>
        Comment,

        /// <summary>
        /// String inside R-type block
        /// </summary>
        String,

        /// <summary>
        /// Number inside R-type block
        /// </summary>
        Number,

        /// <summary>
        /// "#if ... #else" sequence
        /// </summary>
        Pragma,

        /// <summary>
        /// Known language keyword like '\arguments, \author, ...'
        /// </summary>
        Keyword,

        OpenCurlyBrace,
        CloseCurlyBrace,

        OpenSquareBracket,
        CloseSquareBracket,

        /// <summary>
        /// Preudo-token indicating end of stream
        /// </summary>
        EndOfStream
    }
}
