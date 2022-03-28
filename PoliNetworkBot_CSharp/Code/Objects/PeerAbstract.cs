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
    public readonly long id;
    public readonly TLAbsInputPeer peer;
    public readonly ChatType type;

    public PeerAbstract(long id, ChatType type)
    {
        this.id = id;
        this.type = type;
        switch (type)
        {
            case ChatType.Private:
            {
                peer = new TLInputPeerUser { UserId = (int)id };
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
        }
    }

    internal long GetUserId()
    {
        return id;
    }

    internal TLAbsInputPeer GetPeer()
    {
        return peer;
    }
}