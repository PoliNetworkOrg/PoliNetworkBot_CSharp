#region

using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using TeleSharp.TL;
using TLSharp.Core;
using TLSharp.Core.Exceptions;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class UserbotConnect
    {
        internal static async Task<TelegramClient> ConnectAsync(UserBotInfo userbot)
        {
            var apiId = userbot.GetApiId();
            if (apiId == null)
                return null;

            var telegramClient = new TelegramClient(apiId.Value, userbot.GetApiHash(),
                sessionUserId: userbot.GetSessionUserId());
            await telegramClient.ConnectAsync();

            if (telegramClient.IsUserAuthorized())
                return telegramClient;

            var numberToAuthenticate = userbot.GetPhoneNumber();
            var hash = await telegramClient.SendCodeRequestAsync(numberToAuthenticate);
            var code = "";
            var passwordToAuthenticate = userbot.GetPasswordToAuthenticate();


            TLUser user;
            try
            {
                user = await telegramClient.MakeAuthAsync(numberToAuthenticate, hash, code);
            }
            catch (CloudPasswordNeededException ex)
            {
                Console.WriteLine(ex.Message);

                var passwordSetting = await telegramClient.GetPasswordSetting();
                var password = passwordToAuthenticate;

                user = await telegramClient.MakeAuthWithPasswordAsync(passwordSetting, password);
            }
            catch (InvalidPhoneCodeException ex)
            {
                throw new Exception(
                    "CodeToAuthenticate is wrong in the app.config file, fill it with the code you just got now by SMS/Telegram",
                    ex);
            }

            if (user != null && telegramClient.IsUserAuthorized())
                return telegramClient;

            return null;
        }
    }
}