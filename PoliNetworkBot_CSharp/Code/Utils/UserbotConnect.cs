#region

using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using Telegram.Bot;
using TeleSharp.TL;
using TLSharp.Core;
using TLSharp.Core.Exceptions;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class UserbotConnect
{
    internal static async Task<TelegramBotAbstract?> ConnectAsync(BotInfoAbstract userbot)
    {
        switch (userbot.botTypeApi)
        {
            case BotTypeApi.REAL_BOT:
            {
                if (userbot.token != null)
                {
                    TelegramBotClient telegramBotClient = new(userbot.token);
                    return new TelegramBotAbstract(telegramBotClient, userbot.website, userbot.contactString,
                        userbot.botTypeApi.Value, userbot.onMessages, userbot.GithubToken);
                }

                break;
            }

            case BotTypeApi.USER_BOT:
            {
                var apiId = userbot.apiId;
                if (apiId == null)
                    return null;

                TelegramClient? telegramClient = null;
                try
                {
                    telegramClient = new TelegramClient((int)apiId.Value, userbot.apiHash,
                        sessionUserId: userbot.SessionUserId);
                }
                catch
                {
                    // ignored
                }

                if (telegramClient == null)
                    return null;

                await telegramClient.ConnectAsync();

                if (telegramClient.IsUserAuthorized())
                    return new TelegramBotAbstract(telegramClient, userbot.website, userbot.contactString,
                        userbot.userId, userbot.botTypeApi.Value, userbot.onMessages, userbot.GithubToken);

                var numberToAuthenticate = userbot.NumberNumber;
                var hash = await telegramClient.SendCodeRequestAsync(numberToAuthenticate);
                // ReSharper disable once ConvertToConstant.Local
                var code = "";
                var passwordToAuthenticate = userbot.passwordToAuthenticate;

                TLUser user;
                try
                {
                    user = await telegramClient.MakeAuthAsync(numberToAuthenticate, hash, code);
                }
                catch (CloudPasswordNeededException ex)
                {
                    Logger.Logger.WriteLine(ex.Message);

                    var passwordSetting = await telegramClient.GetPasswordSetting();

                    user = await telegramClient.MakeAuthWithPasswordAsync(passwordSetting, passwordToAuthenticate);
                }
                catch (InvalidPhoneCodeException ex)
                {
                    throw new Exception(
                        "CodeToAuthenticate is wrong in the app.config file, fill it with the code you just got now by SMS/Telegram",
                        ex);
                }

                if (user != null && telegramClient.IsUserAuthorized())
                    return new TelegramBotAbstract(telegramClient, userbot.website, userbot.contactString,
                        userbot.userId, userbot.botTypeApi.Value, userbot.onMessages, userbot.GithubToken);

                return null;
            }

            case BotTypeApi.DISGUISED_BOT:
            {
                var apiId = userbot.apiId;
                if (apiId == null)
                    return null;

                var t = new TelegramClient((int)apiId.Value, userbot.apiHash,
                    sessionUserId: userbot.SessionUserId);
                await t.ConnectAsync();

                if (t.IsUserAuthorized())
                    return new TelegramBotAbstract(t, userbot.website, userbot.contactString, userbot.userId,
                        userbot.botTypeApi.Value, userbot.onMessages, userbot.GithubToken);

                var r = await t.AuthImportBotAuthorization(userbot.GetToken());

                return r?.User != null
                    ? new TelegramBotAbstract(t, userbot.website, userbot.contactString, userbot.userId,
                        userbot.botTypeApi, userbot.onMessages, userbot.GithubToken)
                    : null;
            }
        }

        return null;
    }
}