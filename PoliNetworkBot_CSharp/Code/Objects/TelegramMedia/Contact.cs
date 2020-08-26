#region

using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

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

        public override TLAbsInputMedia GetMediaTl()
        {
            return new TLInputMediaContact
            {
                FirstName = _firstName,
                LastName = _lastName, PhoneNumber = _phoneNumber
            };
        }
    }
}