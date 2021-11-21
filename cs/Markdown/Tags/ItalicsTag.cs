﻿using System.Text;

namespace Markdown
{
    public class ItalicsTag : ITag
    {
        public string OpeningMarkup => "_";
        public string ClosingMarkup => "_";
        public string OpeningTag => "<em>";
        public string ClosingTag => "</em>";

        public bool IsBrokenMarkup(string source, int start, int length)
        {
            return HasEmptyBody(length)
                   || HasFirstGap(source, start)
                   || HasLastGap(source, start, length)
                   || IsInDifferentWords(source, start, length)
                   || IsWordWithDigits(source, start, length);
        }

        private bool IsWordWithDigits(string source, int start, int length)
        {
            var startPosition = GetWordStartPosition(source, start);
            var endPosition = GetWordEndPosition(source, start + length - 1);
            for (var i = startPosition; i < endPosition; i++)
                if (int.TryParse(source[i].ToString(), out var result))
                    return true;
            return false;
        }

        private int GetWordStartPosition(string source, int start)
        {
            var result = start;
            while (result > 0 && source[result] != ' ')
                result--;
            return result;
        }
        
        private int GetWordEndPosition(string source, int end)
        {
            var result = end;
            while (result < source.Length - 1 && source[result] != ' ')
                result++;
            return result;
        }

        private bool IsInDifferentWords(string source, int start, int length)
        {
            var end = start + length;
            return HasGaps(source, start, length)
                   && (start > 0 && source[start - 1] != ' '
                       || end < source.Length && source[end] != ' ');
        }

        private bool HasGaps(string source, int start, int length)
        {
            var normStart = start + OpeningMarkup.Length;
            var normEnd = start + length - ClosingMarkup.Length;
            for (var i = normStart; i < normEnd; i++)
                if (source[i] == ' ')
                    return true;
            return false;
        }

        private bool HasEmptyBody(int length)
        {
            return length < OpeningMarkup.Length + ClosingMarkup.Length + 1;
        }

        private bool HasLastGap(string source, int start, int length)
        {
            return source[start + length - 1 - ClosingMarkup.Length] == ' ';
        }

        private bool HasFirstGap(string source, int start)
        {
            return source[start + OpeningMarkup.Length] == ' ';
        }
    }
}