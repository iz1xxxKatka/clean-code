using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class ImageTag : ITag
    {
        private readonly HashSet<char> stopSymbols = new HashSet<char>
        {
            ' ', '\n'
        };

        public string OpeningMarkup => "~";
        public string ClosingMarkup => "~";
        public string OpeningTag => "<img src=\"";
        public string ClosingTag => "\"/>";

        public void Replace(List<string> builder, int start, int end)
        {
            builder[start] = OpeningTag;
            builder[end] = ClosingTag;
        }

        public bool IsBrokenMarkup(string source, int start, int length)
            => HasEmptyBody(length)
               || HasStopSymbols(source, start, length)
               || !IsFullWord(source, start, length);

        private bool HasEmptyBody(int length)
            => length < OpeningMarkup.Length + ClosingMarkup.Length + 1;

        private bool IsFullWord(string source, int start, int length)
            => (start == 0 || stopSymbols.Contains(source[start - 1]))
               && (start + length == source.Length || stopSymbols.Contains(source[start + length]));

        private bool HasStopSymbols(string source, int start, int length)
            => source.Substring(start, length).Any(s => stopSymbols.Contains(s));
    }
}