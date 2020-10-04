using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class WordToBeFirst
    {
        private string word;
        private List<string> similarWords;

        public WordToBeFirst(string word)
        {
            this.word = word;
        }

        public WordToBeFirst(string word, List<string> similarWords) : this(word)
        {
            this.similarWords = similarWords;
        }
    }
}