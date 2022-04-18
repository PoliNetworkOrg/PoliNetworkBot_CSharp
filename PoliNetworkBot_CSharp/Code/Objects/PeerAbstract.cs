#region

using System;
using Newtonsoft.Json;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
internal class PeerAbstract
{
    public readonly long? Id;
    public readonly TLAbsInputPeer Peer;
    public readonly ChatType Type;

    public PeerAbstract(long? id, ChatType type)
    {
        Id = id;
        Type = type;

        switch (type)
        {
            case ChatType.Private:
            {
                Peer = new TLInputPeerUser { UserId = id == null ? default : (int)id };
                break;
            }

            case ChatType.Group:
                break;

            case ChatType.Channel:
                break;

            case ChatType.Supergroup:
                break;

            case ChatType.Sender:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    internal long? GetUserId()
    {
        return Id;
    }

    internal TLAbsInputPeer GetPeer()
    {
        return Peer;
    }
}