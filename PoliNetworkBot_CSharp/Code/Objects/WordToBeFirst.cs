#region

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class WordToBeFirst
{
    private readonly List<string>? _similarWords;
    private readonly string _word;

    public WordToBeFirst(string word)
    {
        _word = word;
    }

    public WordToBeFirst(string word, List<string>? similarWords) : this(word)
    {
        _similarWords = similarWords;
    }

    internal Tuple<bool, string> Matches(string t)
    {
        if (string.IsNullOrEmpty(t))
            return new Tuple<bool, string>(false, _word);

        if (t == _word)
            return new Tuple<bool, string>(true, _word);

        if (_similarWords == null || _similarWords.Count == 0) return new Tuple<bool, string>(false, _word);

        return _similarWords.Any(x => x == t)
            ? new Tuple<bool, string>(true, _word)
            : new Tuple<bool, string>(false, _word);
    }

    internal bool IsTaken(IEnumerable<string?> taken)
    {
        return taken.Any(r => r == _word);
    }

    internal string GetWord()
    {
        return _word;
    }
}