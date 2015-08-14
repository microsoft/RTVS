
namespace Microsoft.R.Support.RD.Tokens
{
    // https://developer.r-project.org/parseRd.pdf

    public enum RdTokenType
    {
        /// <summary>
        /// Unrecognized token
        /// </summary>
        Unknown,

        /// <summary>
        /// % comment, lasts to the end of the line
        /// </summary>
        Comment,

        /// <summary>
        /// "..." sequence
        /// </summary>
        String,

        /// <summary>
        /// "#if ... #else" sequence
        /// </summary>
        Pragma,

        /// <summary>
        /// Known language keyword like '\arguments, \author, ...'
        /// </summary>
        Keyword,

        OpenBrace,
        CloseBrace,

        /// <summary>
        /// Sequence inside { }
        /// </summary>
        Argument,

        /// <summary>
        /// Preudo-token indicating end of stream
        /// </summary>
        EndOfStream
    }
}
