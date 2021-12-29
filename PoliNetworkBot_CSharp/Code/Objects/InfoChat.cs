using System;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class InfoChat
    {
        public InfoChat(Chat messageChat, DateTime now)
        {
            _Chat = messageChat;
            _dateTime = now;
        }

        private DateTime? _dateTime;

        public Chat _Chat;

        public bool IsInhibited()
        {
            if (_dateTime == null)
            {
                UpdateTimeOfLastLinkCheck();
                return true;
            }

            return _dateTime.Value.AddDays(1) >= DateTime.Now;
        }

        public void UpdateTimeOfLastLinkCheck()
        {
            _dateTime = DateTime.Now;
        }
    }
}