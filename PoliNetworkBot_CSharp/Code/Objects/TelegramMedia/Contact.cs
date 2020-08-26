#region

using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia
{
    public class Contact : Media
    {
        private readonly string firstName;
        private readonly string lastName;
        private readonly string phoneNumber;
        private readonly string vCard;

        public Contact(string phoneNumber, string firstName, string lastName, string vCard)
        {
            this.phoneNumber = phoneNumber;
            this.lastName = lastName;
            this.firstName = firstName;
            this.vCard = vCard;
        }

        public override MessageType? GetMediaBotType()
        {
            return MessageType.Contact;
        }

        public override TLAbsInputMedia GetMediaTl()
        {
            return new TLInputMediaContact
            {
                FirstName = firstName,
                LastName = lastName, PhoneNumber = phoneNumber
            };
        }
    }
}