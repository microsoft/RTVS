using System;
using System.Collections.Generic;

namespace Microsoft.R.Support.RD.Tokens
{
    internal class RdBraceCounter<T> where T : IComparable<T>
    {
        T _openCurlyBrace;
        T _closeCurlyBrace;
        T _openSquareBracket;
        T _closeSquareBracket;

        private Stack<T> _curlyBraces = new Stack<T>();
        private Stack<T> _squareBrackets = new Stack<T>();

        public RdBraceCounter(T openCurlyBrace, T closeCurlyBrace): 
            this(openCurlyBrace, closeCurlyBrace, default(T), default(T))
        {
        }

        public RdBraceCounter(T openCurlyBrace, T closeCurlyBrace, T openSquareBracket, T closeSquareBracket)
        {
            _openCurlyBrace = openCurlyBrace;
            _closeCurlyBrace = closeCurlyBrace;

            _openSquareBracket = openSquareBracket;
            _closeSquareBracket = closeSquareBracket;
        }

        public int Count
        {
            get { return _curlyBraces.Count; }
        }

        public bool CountBrace(T brace)
        {
            if (0 == brace.CompareTo(_openCurlyBrace))
            {
                _curlyBraces.Push(brace);
            }
            else if (0 != _openSquareBracket.CompareTo(default(T)) && 0 == brace.CompareTo(_openSquareBracket))
            {
                _squareBrackets.Push(brace);
            }
            else if (0 == brace.CompareTo(_closeCurlyBrace))
            {
                if (_curlyBraces.Count > 0)
                    _curlyBraces.Pop();
            }
            else if (0 != _closeSquareBracket.CompareTo(default(T)) && 0 == brace.CompareTo(_closeSquareBracket))
            {
                if (_squareBrackets.Count > 0)
                    _squareBrackets.Pop();
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
