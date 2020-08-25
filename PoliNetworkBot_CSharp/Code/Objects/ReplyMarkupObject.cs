using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class ReplyMarkupObject
    {
        private readonly List<List<KeyboardButton>> _list;
        private readonly Enums.ReplyMarkupEnum _replyMarkupEnum;

        public ReplyMarkupObject(List<List<KeyboardButton>> list)
        {
            this._list = list;
            this._replyMarkupEnum = ReplyMarkupEnum.CHOICE;
        }

        public ReplyMarkupObject(Enums.ReplyMarkupEnum replyMarkupEnum)
        {
            this._replyMarkupEnum = replyMarkupEnum;
        }

        public IReplyMarkup GetReplyMarkup()
        {
            return _replyMarkupEnum switch
            {
                ReplyMarkupEnum.FORCED => new ForceReplyMarkup(),
                ReplyMarkupEnum.REMOVE => new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardRemove(),
                ReplyMarkupEnum.CHOICE => new ReplyKeyboardMarkup(_list),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}