using System;
using Microsoft.Html.Core.Parser.Tokens;

namespace Microsoft.Html.Core.Parser {
    public sealed partial class HtmlParser {
        internal void OnStyleState(NameToken nameToken) {
            string styleQualifiedName = _cs.GetSubstringAt(nameToken.QualifiedName.Start, nameToken.QualifiedName.Length);
            string endStyle = string.Concat("</", styleQualifiedName);

            // here is interesting and somewhat odd piece... <style> and <script> blocks are artifacts too.
            // However, they cannot be handled through preprocessor since they are regular markup elements. 
            // Worse still, <script runat="server"> is ASP.NET artifact... But since they are really 
            // markup elements, we'll have to deal with them here.
            var range = FindEndOfBlock(endStyle, simpleSearch: true);
            StyleBlockFound?.Invoke(this, new HtmlParserBlockRangeEventArgs(range, String.Empty));
        }
    }
}

