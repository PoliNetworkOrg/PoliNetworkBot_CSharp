#region

using System;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class InfoChat
    {
        public Chat _Chat;

        private DateTime? _dateTime;

        public InfoChat(Chat messageChat, DateTime updatedAt)
        {
            _Chat = messageChat;
            _dateTime = updatedAt;
        }

        public bool IsInhibited()
        {
            if (_dateTime != null) return _dateTime.Value.AddDays(1) >= DateTime.Now;
            UpdateTimeOfLastLinkCheck();
            return true;
        }

        public void UpdateTimeOfLastLinkCheck()
        {
            _dateTime = DateTime.Now;
        }
    }
}