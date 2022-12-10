#region

using System.IO;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Utils.Logger;

//using InstagramApiSharp.API.Builder;
//using InstagramApiSharp.Classes;
//using InstagramApiSharp.Logger;
//using Minista.Helpers;
//using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.API;
//using PoliNetworkBot_CSharp.Code.IG.Minista.Helpers.Uploaders;

#endregion

namespace PoliNetworkBot_CSharp.Code.Beta.IG;

internal static class TestIg
{
    //https://github.com/ramtinak/InstagramApiSharp

    public static async Task<bool> MainIgAsync()
    {
        Logger.WriteLine("Starting demo of InstagramApiSharp project");

        string[] c;
        try
        {
            c = await File.ReadAllLinesAsync(Paths.Ig.Credentials);
        }
        catch
        {
            await File.WriteAllTextAsync(Paths.Ig.Credentials, "user\npassword");
            c = new[] { "user", "password" };
        }

        return c.Length >= 2 && c[0] != "user";

        // create user session data and provide login details
        //var userSession = new UserSessionData
        //{
        //    UserName = c[0].Trim(),
        //    Password = c[1].Trim()
        //};
        // if you want to set custom device (user-agent) please check this:
        // https://github.com/ramtinak/InstagramApiSharp/wiki/Set-custom-device(user-agent)

        //var delay = RequestDelay.FromSeconds(2, 2);
        // create new InstaApi instance using Builder
        //var x = InstaApiBuilder.CreateBuilder()
        //    .SetUser(userSession)
        //    .UseLogger(new DebugLogger(LogLevel.All)) // use logger for requests and debug messages
        //    .SetRequestDelay(delay)
        //    .Build();
        // create account

        //if (x.IsUserAuthenticated) return true;
        // login
        //Logger.WriteLine($"Logging in as {userSession.UserName}");
        //delay.Disable();
        //var logInResult = await x.LoginAsync();
        //delay.Enable();
        //if (logInResult.Succeeded)
        //{
        //    await DoThingsAsync(x);
        //    return true;
        //}

        //if (logInResult.Value != InstaLoginResult.ChallengeRequired)
        //    return true;

        //var challenge = await x.GetChallengeRequireVerifyMethodAsync();
        //if (challenge != null && challenge.Succeeded)
        //{
        //    if (challenge.Value != null && challenge.Value.SubmitPhoneRequired)
        //   {
        //      ;
        //  }
        //  else
        //  {
        //      if (challenge.Value?.StepData != null)
        //      {
        //          if (!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber))
        //          {
        //              ;
        //              Console.WriteLine("!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber)");
        //          }
//
        //                  if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
        //          {
        //              ;
        //              Console.WriteLine("!string.IsNullOrEmpty(challenge.Value.StepData.Email)");
        //              }
//
        //                  ;
        //      }
        //      }
        //  }
        //  else
        //  {
        //      ;
        //  }
//
        //      ;
    }

    // private static async Task DoThingsAsync(InstaApi x)
    //{
    //  var album = new PhotoAlbumUploader();
    //  var file = await StorageFile.GetFileFromPathAsync("test.jpg");
    //  StorageFile[] files = { file };
    //  string? caption = null;
    //  await album.SetFiles(files, caption, x);
    //  ;
    //}
}