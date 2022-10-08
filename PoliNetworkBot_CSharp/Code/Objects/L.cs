#region

using System;
using System.Collections.Generic;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

/// <summary>
///           ___
///                _-_-  _/\______\\__
///             _-_-__  / ,-. -|-  ,-.`-.
///         whoosh _-_- `( o )----( o )-'
///                       `-'      `-'
/// </summary>
public class L: Language
{
    public L(string langCode, string text): base(new Dictionary<string, string?>
    { { langCode, text } }) {}
    
    public L(string langCode1, string text1, string langCode2, string text2): base(new Dictionary<string, string?>
    {
        { langCode1, text1 },
        { langCode2, text2 }
    })
    {}
}