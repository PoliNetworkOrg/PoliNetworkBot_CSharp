#region

using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;

public class Contact : GenericFile
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

#pragma warning disable CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono

    public override async Task<TlFileToSend> GetMediaTl(TelegramClient telegramClient)
#pragma warning restore CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono
    {
        var r = new TLInputMediaContact
        {
            FirstName = _firstName,
            LastName = _lastName,
            PhoneNumber = _phoneNumber
        };

        return new TlFileToSend(r);
    }
}