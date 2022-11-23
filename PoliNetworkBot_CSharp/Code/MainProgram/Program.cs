#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using PoliNetworkBot_CSharp.Test.IG;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

#endregion

namespace PoliNetworkBot_CSharp.Code.MainProgram;

internal static class Program
{


    private static async Task Main(string[]? args)
    {
        var toExit = Utils.Main.ProgramUtil.FirstThingsToDo();
        if (toExit == ToExit.EXIT)
        {
            Logger.WriteLine("Program will stop.");
            return;
        }

        while (true)
        {
            var (item1, item2) = Utils.Main.ProgramUtil.MainGetMenuChoice2(args);

            switch (item1)
            {
                case '1': //reset everything
                {
                    Utils.Main.ProgramUtil.ResetEverything(true);

                    return;
                }

                case '2': //normal mode
                case '3': //disguised bot test
                case '8':
                case '9':
                {
                    Utils.Main.ProgramUtil.MainBot(item1, item2);
                    return;
                }

                case '4':
                {
                    Utils.Main.ProgramUtil.ResetEverything(false);
                    return;
                }

                case '5':
                {
                    _ = await TestIg.MainIgAsync();
                    return;
                }

                case '6':
                {
                    NewConfig.NewConfigMethod(true, false, false, false, false);
                    return;
                }
                case '7':
                {
                    NewConfig.NewConfigMethod(false, false, true, false, false);
                    return;
                }
                case 't':
                {
                    try
                    {
                        //SpamTest.Main2();
                        //Test_CheckLink.Test_CheckLink2();
                        await TestIg.MainIgAsync();
                        return;
                    }
                    catch
                    {
                        // ignored
                    }

                    break;
                }
            }
        }
    }

}