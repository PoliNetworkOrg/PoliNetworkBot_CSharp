#region

using System;
using Newtonsoft.Json;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

/// <summary>
///     A Telegram Message Wrapper
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MessageEventArgs
{
    public readonly Message Message;
<<<<<<< HEAD
=======

>>>>>>> dev
    public bool Edit;


    public MessageEventArgs(Message message, bool edit = false)
    {
        Message = message;
<<<<<<< HEAD
=======

>>>>>>> dev
        Edit = edit;
    }
}