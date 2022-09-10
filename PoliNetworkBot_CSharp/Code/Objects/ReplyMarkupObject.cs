#region

using System;
using PoliNetworkBot_CSharp.Code.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class ReplyMarkupObject
{
    private readonly InlineKeyboardMarkup? _inlineKeyboardMarkup;
    private readonly ReplyMarkupOptions? _list;
    private readonly ReplyMarkupEnum _replyMarkupEnum;

    public ReplyMarkupObject(ReplyMarkupOptions? list)
    {
        if (list == null)
            return;
        _list = list;
        _replyMarkupEnum = ReplyMarkupEnum.CHOICE;
    }

    public ReplyMarkupObject(ReplyMarkupEnum replyMarkupEnum)
    {
        _replyMarkupEnum = replyMarkupEnum;
    }

    public ReplyMarkupObject(InlineKeyboardMarkup? inlineKeyboardMarkup)
    {
        if (inlineKeyboardMarkup == null)
        {
            _replyMarkupEnum = ReplyMarkupEnum.REMOVE;
            return;
        }

        _inlineKeyboardMarkup = inlineKeyboardMarkup;
        _replyMarkupEnum = ReplyMarkupEnum.INLINE;
    }

    public IReplyMarkup? GetReplyMarkupBot()
    {
        return _replyMarkupEnum switch
        {
            ReplyMarkupEnum.FORCED => new ForceReplyMarkup(),
            ReplyMarkupEnum.REMOVE => new ReplyKeyboardRemove(),
            ReplyMarkupEnum.CHOICE => KeyboardMarkupMethod(),
            ReplyMarkupEnum.INLINE => _inlineKeyboardMarkup,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private IReplyMarkup? KeyboardMarkupMethod()
    {
        var matrixKeyboardButton = _list?.GetMatrixKeyboardButton();
        if (matrixKeyboardButton != null)
            return new ReplyKeyboardMarkup(matrixKeyboardButton!)
                { OneTimeKeyboard = true };

        return null;
    }

    public TLAbsReplyMarkup? GetReplyMarkupUserBot()
    {
        if (_list != null)
            return _replyMarkupEnum switch
            {
                ReplyMarkupEnum.FORCED => new TLReplyKeyboardForceReply(),
                ReplyMarkupEnum.REMOVE => new TLReplyKeyboardHide(),
                ReplyMarkupEnum.CHOICE => new TLReplyKeyboardMarkup { Rows = _list.GetMatrixTlKeyboardButton() },
                ReplyMarkupEnum.INLINE => GetRowsInline(_inlineKeyboardMarkup),
                _ => throw new ArgumentOutOfRangeException()
            };
        return null;
    }

    private static TLReplyInlineMarkup GetRowsInline(InlineKeyboardMarkup? inlineKeyboardMarkup)
    {
        TLVector<TLKeyboardButtonRow>? r2 = null;

        if (inlineKeyboardMarkup != null)
            foreach (var x1 in inlineKeyboardMarkup.InlineKeyboard)
            {
                var buttons = new TLVector<TLAbsKeyboardButton>();

                foreach (var x2 in x1)
                {
                    var x3 = new TLKeyboardButton { Text = x2.Text };
                    buttons.Add(x3);
                }

                var tLKeyboardButtonRow = new TLKeyboardButtonRow { Buttons = buttons };
                r2?.Add(tLKeyboardButtonRow);
            }

        var r = new TLReplyInlineMarkup { Rows = r2 };
        return r;
    }

    internal InlineKeyboardMarkup? ToInlineKeyBoard()
    {
        return _inlineKeyboardMarkup;
    }
}