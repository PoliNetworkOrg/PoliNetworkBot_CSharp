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
    internal class UserbotConnect
    {
        internal static async Task<TelegramClient> ConnectAsync(UserBotInfo userbot)
        {
            var api_id = userbot.GetApiId();
            if (api_id == null)
                return null;

            var telegramClient = new TelegramClient(api_id.Value, userbot.GetApiHash(),
                sessionUserId: userbot.GetSessionUserId());
            await telegramClient.ConnectAsync();

            if (telegramClient.IsUserAuthorized())
                return telegramClient;

            var NumberToAuthenticate = userbot.GetPhoneNumber();
            var hash = await telegramClient.SendCodeRequestAsync(NumberToAuthenticate);
            var code = "";
            var PasswordToAuthenticate = userbot.GetPasswordToAuthenticate();
            ;

            TLUser user;
            try
            {
                user = await telegramClient.MakeAuthAsync(NumberToAuthenticate, hash, code);
            }
            catch (CloudPasswordNeededException ex)
            {
                Console.WriteLine(ex.Message);

                var passwordSetting = await telegramClient.GetPasswordSetting();
                var password = PasswordToAuthenticate;

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