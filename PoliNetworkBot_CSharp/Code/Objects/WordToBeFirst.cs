using System;
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

        internal Tuple<bool,string> Matches(string t)
        {
            if (string.IsNullOrEmpty(t))
                return new Tuple<bool, string>(false, word);

            if (t == word)
                return new Tuple<bool, string>(true, word);

            if (similarWords == null || similarWords.Count == 0)
            {
                return new Tuple<bool, string>(false, word);
            }

            foreach (var x in similarWords)
            {
                if (x == t)
                    return new Tuple<bool, string>(true, word);
            }

            return new Tuple<bool, string>(false, word);
        }

        internal bool IsTaken(List<string> taken)
        {
            foreach (var r in taken)
            {
                if (r == this.word)
                    return true;
            }

            return false;
        }

        internal string GetWord()
        {
            return this.word;
        }
    }
}