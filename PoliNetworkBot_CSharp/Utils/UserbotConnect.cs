using System;
using Telegram.Bot.Types;
using TLSharp.Core;
using TLSharp.Core.Exceptions;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class UserbotConnect
    {
        internal static async System.Threading.Tasks.Task<TelegramClient> ConnectAsync(UserBotInfo userbot)
        {
            var api_id = userbot.GetApiId();
            if (api_id == null)
                return null;

            TelegramClient telegramClient =  new TLSharp.Core.TelegramClient(api_id.Value, userbot.GetApiHash(), sessionUserId: userbot.GetSessionUserId());
            await telegramClient.ConnectAsync();

            if (telegramClient.IsUserAuthorized())
                return telegramClient;

            string NumberToAuthenticate = userbot.GetPhoneNumber();
            var hash = await telegramClient.SendCodeRequestAsync(NumberToAuthenticate);
            string code = "";
            string PasswordToAuthenticate = userbot.GetPasswordToAuthenticate();
            ;

            TeleSharp.TL.TLUser user;
            try
            {
                user = await telegramClient.MakeAuthAsync(NumberToAuthenticate, hash, code);
            }
            catch (CloudPasswordNeededException ex)
            {
                var passwordSetting = await telegramClient.GetPasswordSetting();
                var password = PasswordToAuthenticate;

                user = await telegramClient.MakeAuthWithPasswordAsync(passwordSetting, password);
            }
            catch (InvalidPhoneCodeException ex)
            {
                throw new Exception("CodeToAuthenticate is wrong in the app.config file, fill it with the code you just got now by SMS/Telegram",
                                    ex);
            }

            if (user != null && telegramClient.IsUserAuthorized())
                return telegramClient;

            return null;
        }
    }
}