using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System;
using System.IO;

namespace PoliNetworkBot_CSharp.Test.IG
{
    internal class Test_IG
    {
        //https://github.com/ramtinak/InstagramApiSharp

        public static async System.Threading.Tasks.Task<bool> MainIGAsync()
        {
            Console.WriteLine("Starting demo of InstagramApiSharp project");

            string[] c = null;
            try
            {
                c = File.ReadAllLines(PoliNetworkBot_CSharp.Code.Data.Constants.Paths.IG.CREDENTIALS);
            }
            catch
            {
                File.WriteAllText(Code.Data.Constants.Paths.IG.CREDENTIALS, "");
            }

            if (c == null || c.Length < 2)
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

            if (!x.IsUserAuthenticated)
            {
                // login
                Console.WriteLine($"Logging in as {userSession.UserName}");
                delay.Disable();
                var logInResult = await x.LoginAsync();
                delay.Enable();
                if (!logInResult.Succeeded)
                {
                    if (logInResult.Value == InstaLoginResult.ChallengeRequired)
                    {
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
                                    }
                                    if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
                                    {
                                        ;
                                    }

                                    ;
                                }
                            }
                        }
                        else
                        {
                            ;
                        }
                    }
                }

                ;

                return true;
            }

            return true;
        }
    }
}