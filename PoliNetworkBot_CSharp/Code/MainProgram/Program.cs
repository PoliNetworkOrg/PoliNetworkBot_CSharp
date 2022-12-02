#region

using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.Main;
using PoliNetworkBot_CSharp.Code.Utils.Restore;
using PoliNetworkBot_CSharp.Test.IG;

#endregion

namespace PoliNetworkBot_CSharp.Code.MainProgram;

internal static class Program
{
    private static async Task Main(string[]? args)
    {
        var toExit = ProgramUtil.FirstThingsToDo();
        if (toExit == ToExit.EXIT)
        {
            Logger.WriteLine("Program will stop.");
            return;
        }

        while (true)
        {
            var (item1, item2) = ProgramUtil.MainGetMenuChoice2(args);

            switch (item1)
            {
                case '1': //reset everything
                {
                    ProgramUtil.ResetEverything(true);

                    return;
                }

                case '2': //normal mode
                case '3': //disguised bot test
                case '8':
                case '9':
                {
                    ProgramUtil.MainBot(item1, item2);
                    return;
                }

                case '4':
                {
                    ProgramUtil.ResetEverything(false);
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
                        //await TestIg.MainIgAsync();
                        await PoliNetworkBot_CSharp.Test.Test.MainTest();
                        return;
                    }
                    catch
                    {
                        // ignored
                    }

                    break;
                }

                case 'r':
                {
                    await RestoreDbUtil.RestoreDb();
                    ProgramUtil.MainBot(item1, item2);
                    break;
                }
            }
        }
    }
}