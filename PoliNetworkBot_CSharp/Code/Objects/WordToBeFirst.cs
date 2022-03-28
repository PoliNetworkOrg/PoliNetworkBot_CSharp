#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class WordToBeFirst
{
    private readonly List<string> similarWords;
    private readonly string word;

    public WordToBeFirst(string word)
    {
        this.word = word;
    }

    public WordToBeFirst(string word, List<string> similarWords) : this(word)
    {
        this.similarWords = similarWords;
    }

    internal Tuple<bool, string> Matches(string t)
    {
        if (string.IsNullOrEmpty(t))
            return new Tuple<bool, string>(false, word);

        if (t == word)
            return new Tuple<bool, string>(true, word);

        if (similarWords == null || similarWords.Count == 0) return new Tuple<bool, string>(false, word);

        return similarWords.Any(x => x == t)
            ? new Tuple<bool, string>(true, word)
            : new Tuple<bool, string>(false, word);
    }

    internal bool IsTaken(IEnumerable<string> taken)
    {
        return taken.Any(r => r == word);
    }

    internal string GetWord()
    {
        return word;
    }
}