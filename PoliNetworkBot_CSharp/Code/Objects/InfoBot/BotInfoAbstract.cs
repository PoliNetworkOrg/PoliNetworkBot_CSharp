#region

using System;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Bots.Materials;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;
using SampleNuGet.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.InfoBot;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class BotInfoAbstract
{
    public bool? acceptedMessages;
    public string? apiHash;
    public long? apiId;
    public BotProgramTypeEnum? botProgramType;
    public BotTypeApi? botTypeApi;
    public string? contactString;
    public DbConfig? DbConfig;
    public string? method;
    public string? NumberCountry;
    public string? NumberNumber;
    public string? onMessages;
    public string? passwordToAuthenticate;
    public string? SessionUserId;
    public string? token;
    public BotProgramTypeEnum TypeEnum; //todo
    public long? userId;
    public string? website;

    internal EventHandler<CallbackQueryEventArgs>? GetCallbackEvent()
    {
        if (onMessages == BotStartMethods.Anon.Item1)
            return CallbackUtils.CallbackMethodStart;
        if (onMessages == BotStartMethods.Moderation.Item1)
            return CallbackUtils.CallbackMethodStart;
        return onMessages == BotStartMethods.Material.Item1 ? Program.BotOnCallbackQueryReceived : null;
    }

    internal string? GetToken()
    {
        return token;
    }

    internal OnMessageMethodObject GetOnMessage()
    {
        TrySetBotType();
        try
        {
            var s = onMessages;
            var r1 = BotStartMethods.GetMethodFromString(s);
            return new OnMessageMethodObject(r1, s);
        }
        catch
        {
            // ignored
        }

        return new OnMessageMethodObject(null, null);
    }

    private void TrySetBotType()
    {
        if (onMessages == BotStartMethods.Moderation.Item1)
            botProgramType = BotProgramTypeEnum.MODERATION;
        else if (onMessages == BotStartMethods.Anon.Item1)
            botProgramType = BotProgramTypeEnum.ANON;
        else if (onMessages == BotStartMethods.Material.Item1)
            botProgramType = BotProgramTypeEnum.MATERIALS;
        else if (onMessages == BotStartMethods.Admin.Item1)
            botProgramType = BotProgramTypeEnum.ADMIN;
        else if (onMessages == BotStartMethods.Primo.Item1)
            botProgramType = BotProgramTypeEnum.PRIMO;
        else if (onMessages == BotStartMethods.Rooms.Item1)
            botProgramType = BotProgramTypeEnum.ROOMS;
        else
            botProgramType = null;
    }

    internal bool? AcceptsMessages()
    {
        return acceptedMessages;
    }

    internal string? GetWebsite()
    {
        try
        {
            return website;
        }
        catch
        {
            return null;
        }
    }

    internal string? GetContactString()
    {
        try
        {
            return contactString;
        }
        catch
        {
            return null;
        }
    }
}