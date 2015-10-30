namespace Microsoft.Markdown.Editor.Tokens {
    /// <summary>
    /// Main regular markdown tokenizer. R Markdown has 
    /// a separate tokenizer.
    /// https://help.github.com/articles/markdown-basics/
    /// </summary>
    internal class RmdTokenizer : MdTokenizer {

        protected override void HandleCharacter() {
            while (!_cs.IsEndOfStream()) {
                bool handled = false;

                // Regular content is Latex-like
                switch (_cs.CurrentChar) {
                    case '#':
                        handled = HandleHeading();
                        break;

                    case '*':
                        handled = HandleStar();
                        break;

                    case '_':
                        if (!char.IsWhiteSpace(_cs.NextChar)) {
                            handled = HandleItalic('_', MarkdownTokenType.Italic);
                        }
                        break;

                    case '>':
                        handled = HandleQuote();
                        break;

                    case '`':
                        handled = HandleBackTick();
                        break;

                    case '-':
                        if (_cs.NextChar == ' ') {
                            handled = HandleListItem();
                        } else if (_cs.NextChar == '-' && _cs.LookAhead(2) == '-') {
                            handled = HandleHeading();
                        }
                        break;

                    case '=':
                        if (_cs.NextChar == '=' && _cs.LookAhead(2) == '=') {
                            handled = HandleHeading();
                        }
                        break;

                    case '[':
                        handled = HandleAltText();
                        break;

                    default:
                        if (_cs.IsDecimal()) {
                            handled = HandleNumberedListItem();
                        }
                        break;

                }

                if (!handled) {
                    _cs.MoveToNextChar();
                }
            }
        }
    }
}
