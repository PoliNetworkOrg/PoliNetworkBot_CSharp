#region

using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class ReplyMarkupOptions
{
    private readonly List<List<KeyboardButton?>> _list;

    public ReplyMarkupOptions(List<List<KeyboardButton?>> list)
    {
        _list = list;
    }

    public IEnumerable<IEnumerable<KeyboardButton?>> GetMatrixKeyboardButton()
    {
        return _list;
    }

    public TLVector<TLKeyboardButtonRow> GetMatrixTlKeyboardButton()
    {
        var r = new TLVector<TLKeyboardButtonRow>();
        foreach (var v1 in _list)
        {
            var buttons = new TLVector<TLAbsKeyboardButton>();

            foreach (var v2 in v1.Where(v2 => v2 != null))
                buttons.Add(new TLKeyboardButton { Text = v2?.Text });

            var row = new TLKeyboardButtonRow { Buttons = buttons };
            r.Add(row);
        }

        return r;
    }
}