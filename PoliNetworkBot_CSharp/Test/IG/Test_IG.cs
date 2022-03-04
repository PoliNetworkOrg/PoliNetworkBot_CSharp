#region

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using Minista.Helpers;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Test.IG
{
    internal class Test_IG
    {
        //https://github.com/ramtinak/InstagramApiSharp

        public static async Task<bool> MainIGAsync()
        {
            Logger.WriteLine("Starting demo of InstagramApiSharp project");

            string[] c = null;
            try
            {
                c = await File.ReadAllLinesAsync(Paths.IG.CREDENTIALS);
            }
            catch
            {
                await File.WriteAllTextAsync(Paths.IG.CREDENTIALS, "user\npassword");
                c = new[] { "user", "password" };
            }

            ;

            if (c == null || c.Length < 2 || c[0] == "user")
                return false;

            // create user session data and provide login details
            var userSession = new UserSessionData
            {
                UserName = c[0].Trim(),
                Password = c[1].Trim()
            };
            // if you want to set custom device (user-agent) please check this:
            // https://github.com/ramtinak/InstagramApiSharp/wiki/Set-custom-device(user-agent)

            var delay = RequestDelay.FromSeconds(2, 2);
            // create new InstaApi instance using Builder
            var x = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.All)) // use logger for requests and debug messages
                .SetRequestDelay(delay)
                .Build();
            // create account

            ;

            if (x.IsUserAuthenticated) return true;
            // login
            Logger.WriteLine($"Logging in as {userSession.UserName}");
            delay.Disable();
            var logInResult = await x.LoginAsync();
            delay.Enable();
            if (logInResult.Succeeded)
            {
                DoThings(x);
                return true;
            }

            ;

            if (logInResult.Value != InstaLoginResult.ChallengeRequired)
                return true;

            var challenge = await x.GetChallengeRequireVerifyMethodAsync();
            if (challenge.Succeeded)
            {
                if (challenge.Value.SubmitPhoneRequired)
                {
                    ;
                }
                else
                {
                    if (challenge.Value.StepData != null)
                    {
                        if (!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber))
                        {
                            ;
                            Console.WriteLine("!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber)");
                        }

                        if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
                        {
                            ;
                            Console.WriteLine("!string.IsNullOrEmpty(challenge.Value.StepData.Email)");
                        }

                        ;
                    }
                }
            }
            else
            {
                ;
            }

            ;

            return true;
        }

        private static void DoThings(InstaApi x)
        {
            var album = new PhotoAlbumUploader();
            StorageFile[] files = null;
            string caption = null;
            album.SetFiles(files, caption, x);
            var singlePhotoUploader = new SinglePhotoUploader(album, x);
            singlePhotoUploader.UploadSinglePhoto();
        }
    }
}