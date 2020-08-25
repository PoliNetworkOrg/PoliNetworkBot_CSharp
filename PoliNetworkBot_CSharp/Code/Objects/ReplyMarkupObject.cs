#region

using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class ReplyMarkupObject
    {
        private readonly List<List<KeyboardButton>> _list;
        private readonly ReplyMarkupEnum _replyMarkupEnum;

        public ReplyMarkupObject(List<List<KeyboardButton>> list)
        {
            _list = list;
            _replyMarkupEnum = ReplyMarkupEnum.CHOICE;
        }

        public ReplyMarkupObject(ReplyMarkupEnum replyMarkupEnum)
        {
            _replyMarkupEnum = replyMarkupEnum;
        }

        public IReplyMarkup GetReplyMarkup()
        {
            return _replyMarkupEnum switch
            {
                ReplyMarkupEnum.FORCED => new ForceReplyMarkup(),
                ReplyMarkupEnum.REMOVE => new ReplyKeyboardRemove(),
                ReplyMarkupEnum.CHOICE => new ReplyKeyboardMarkup(_list),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}