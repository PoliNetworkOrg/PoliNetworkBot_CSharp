#region

using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class ReplyMarkupObject
    {
        private readonly ReplyMarkupOptions _list;
        private readonly ReplyMarkupEnum _replyMarkupEnum;

        public ReplyMarkupObject(ReplyMarkupOptions list)
        {
            _list = list;
            _replyMarkupEnum = ReplyMarkupEnum.CHOICE;
        }

        public ReplyMarkupObject(ReplyMarkupEnum replyMarkupEnum)
        {
            _replyMarkupEnum = replyMarkupEnum;
        }

        public IReplyMarkup GetReplyMarkupBot()
        {
            return _replyMarkupEnum switch
            {
                ReplyMarkupEnum.FORCED => new ForceReplyMarkup(),
                ReplyMarkupEnum.REMOVE => new ReplyKeyboardRemove(),
                ReplyMarkupEnum.CHOICE => new ReplyKeyboardMarkup(_list.GetMatrixKeyboardButton()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public TLAbsReplyMarkup GetReplyMarkupUserBot()
        {
            return _replyMarkupEnum switch
            {
                ReplyMarkupEnum.FORCED => new TeleSharp.TL.TLReplyKeyboardForceReply(),
                ReplyMarkupEnum.REMOVE => new TLReplyKeyboardHide(),
                ReplyMarkupEnum.CHOICE => new TLReplyKeyboardMarkup() { Rows = _list.GetMatrixTlKeyboardButton()},
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}