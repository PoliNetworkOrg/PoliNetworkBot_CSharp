using System;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class InfoChat
    {
        public InfoChat(Chat messageChat, DateTime updatedAt)
        {
            _Chat = messageChat;
            _dateTime = updatedAt;
        }

        private DateTime? _dateTime;

        public Chat _Chat { get; set; }

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