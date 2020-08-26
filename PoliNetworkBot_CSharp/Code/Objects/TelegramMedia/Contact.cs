#region

using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia
{
    public class Contact : Media
    {
        private readonly string _firstName;
        private readonly string _lastName;
        private readonly string _phoneNumber;
        private readonly string _vCard;

        public Contact(string phoneNumber, string firstName, string lastName, string vCard)
        {
            _phoneNumber = phoneNumber;
            _lastName = lastName;
            _firstName = firstName;
            _vCard = vCard;
        }

        public override MessageType? GetMediaBotType()
        {
            return MessageType.Contact;
        }

        public override async Task<TlFileToSend> GetMediaTl(TelegramClient telegramClient)
        {
            var r = new TLInputMediaContact
            {
                FirstName = _firstName,
                LastName = _lastName, PhoneNumber = _phoneNumber
            };
            
            return new TlFileToSend(r);
        }
    }
}