using System;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.BraceMatch {
    internal sealed class RBraceMatcher : BraceMatcher<RTokenType>
    {
        static RBraceMatcher()
        {
            BraceTypeToTokenTypeMap.Add(BraceType.Curly, new Tuple<RTokenType, RTokenType>(RTokenType.OpenCurlyBrace, RTokenType.CloseCurlyBrace));
            BraceTypeToTokenTypeMap.Add(BraceType.Parenthesis, new Tuple<RTokenType, RTokenType>(RTokenType.OpenBrace, RTokenType.CloseBrace));
            BraceTypeToTokenTypeMap.Add(BraceType.Square, new Tuple<RTokenType, RTokenType>(RTokenType.OpenSquareBracket, RTokenType.CloseSquareBracket));
        }

        public RBraceMatcher(ITextView textView, ITextBuffer textBuffer) : base(textView, textBuffer)
        {
        }

        public override bool GetLanguageBracesFromPosition(
            BraceType braceType, 
            int position, bool reversed, out int start, out int end)
        {
            RTokenType startTokenType = BraceTypeToTokenTypeMap[braceType].Item1;
            RTokenType endTokenType = BraceTypeToTokenTypeMap[braceType].Item2;

            RTokenizer tokenizer = new RTokenizer();
            ITextProvider tp = new TextProvider(TextBuffer.CurrentSnapshot);
            IReadOnlyTextRangeCollection<RToken> tokens = tokenizer.Tokenize(tp, 0, tp.Length);

            start = -1;
            end = -1;

            Stack<RTokenType> stack = new Stack<RTokenType>();

            int startIndex = -1;
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].Start == position)
                {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex < 0)
                return false;

            if (tokens[startIndex].TokenType != startTokenType && tokens[startIndex].TokenType != endTokenType)
                return false;

            if (!reversed)
            {
                for (int i = startIndex; i < tokens.Length; i++)
                {
                    RToken token = tokens[i];

                    if (token.TokenType == startTokenType)
                    {
                        stack.Push(token.TokenType);
                    }
                    else if (token.TokenType == endTokenType)
                    {
                        if (stack.Count > 0)
                            stack.Pop();

                        if (stack.Count == 0)
                        {
                            start = tokens[startIndex].Start;
                            end = token.Start;
                            return true;
                        }
                    }
                }
            }
            else
            {
                for (int i = startIndex; i >= 0; i--)
                {
                    RToken token = tokens[i];

                    if (token.TokenType == endTokenType)
                    {
                        stack.Push(token.TokenType);
                    }
                    else if (token.TokenType == startTokenType)
                    {
                        if (stack.Count > 0)
                            stack.Pop();

                        if (stack.Count == 0)
                        {
                            start = token.Start;
                            end = token.Start;

                            end = tokens[startIndex].Start;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
