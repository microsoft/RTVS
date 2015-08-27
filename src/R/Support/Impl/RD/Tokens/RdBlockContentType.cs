using System;

namespace Microsoft.R.Support.RD.Tokens
{
    internal enum BlockContentType
    {
        Latex,
        R,
        Verbatim
    };

    internal static class RdBlockContentType
    {
        internal static string[] _rKeywords = new string[]
        {
            @"\code",
            @"\dontshow",
            @"\donttest",
            @"\examples",
            @"\Sexpr",
            @"\testonly",
            @"\usage",
        };

        internal static string[] _verbatimKeywords = new string[]
         {
            @"\alias",
            @"\deqn",
            @"\dontrun",
            @"\env",
            @"\eqn",
            @"\href",
            @"\kbd",
            @"\newcommand",
            @"\option",
            @"\out",
            @"\preformatted",
            @"\RdOpts",
            @"\Rdversion",
            @"\renewcommand",
            @"\samp",
            @"\special",
            @"\synopsis",
            @"\testonly",
            @"\url",
            @"\verb",
         };

        public static BlockContentType GetBlockContentType(string keyword)
        {
            int index = Array.BinarySearch(_rKeywords, keyword);
            if (index >= 0)
            {
                return BlockContentType.R;
            }

            index = Array.BinarySearch(_verbatimKeywords, keyword);
            if (index >= 0)
            {
                return BlockContentType.Verbatim;
            }

            return BlockContentType.Latex;
        }
    }
}
