#region

using PoliNetworkBot_CSharp.Code.Enums;
using System.Collections.Generic;
using System.Data;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

internal class BanUnbanAllResult
{
    private readonly List<DataRow> done;
    private readonly List<DataRow> failed;

    public BanUnbanAllResult(List<DataRow> done, List<DataRow> failed)
    {
        this.done = done;
        this.failed = failed;
    }

    internal Language GetLanguage(RestrictAction ban_true_unban_false, string target1, long nExceptions)
    {
        var target = "<a href=\"tg://user?id=" + target1 + "\">" + target1 + "</a>";
        switch (ban_true_unban_false)
        {
            case RestrictAction.UNBAN:
                {
                    var text2 = new Language(new Dictionary<string, string>
                {
                    {
                        "en",
                        "Target " + target + " unbanned from " + done.Count + " groups" + "\n" +
                        "not unbanned from " + failed.Count + " groups" + "\n" +
                        "exception number = " + nExceptions
                    },

                    {
                        "it",
                        "Target " + target + " sbannato da " + done.Count + " gruppi" + "\n" +
                        "non sbannato da " + failed.Count + " gruppi" + "\n" +
                        "numero eccezioni = " + nExceptions
                    }
                });

                    return text2;
                }

            case RestrictAction.BAN:
                {
                    var text2 = new Language(new Dictionary<string, string>
                {
                    {
                        "en",
                        "Target " + target + " banned from " + done.Count + " groups" + "\n" +
                        "not banned from " + failed.Count + " groups" + "\n" +
                        "exception number = " + nExceptions
                    },

                    {
                        "it",
                        "Target " + target + " bannato da " + done.Count + " gruppi" + "\n" +
                        "non bannato da " + failed.Count + " gruppi" + "\n" +
                        "numero eccezioni = " + nExceptions
                    }
                });

                    return text2;
                }

            case RestrictAction.MUTE:
                {
                    var text2 = new Language(new Dictionary<string, string>
                {
                    {
                        "en",
                        "Target " + target + " muted from " + done.Count + " groups" + "\n" +
                        "not muted from " + failed.Count + " groups" + "\n" +
                        "exception number = " + nExceptions
                    },

                    {
                        "it",
                        "Target " + target + " mutato da " + done.Count + " gruppi" + "\n" +
                        "non mutato da " + failed.Count + " gruppi" + "\n" +
                        "numero eccezioni = " + nExceptions
                    }
                });

                    return text2;
                }
            case RestrictAction.UNMUTE:
                {
                    var text2 = new Language(new Dictionary<string, string>
                {
                    {
                        "en",
                        "Target " + target + " unmuted from " + done.Count + " groups" + "\n" +
                        "not unmuted from " + failed.Count + " groups" + "\n" +
                        "exception number = " + nExceptions
                    },

                    {
                        "it",
                        "Target " + target + " smutato da " + done.Count + " gruppi" + "\n" +
                        "non smutato da " + failed.Count + " gruppi" + "\n" +
                        "numero eccezioni = " + nExceptions
                    }
                });

                    return text2;
                }
        }

        return null;
    }

    internal List<DataRow> GetSuccess()
    {
        return done;
    }

    internal List<DataRow> GetFailed()
    {
        return failed;
    }
}