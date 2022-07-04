﻿#region

using System.Collections.Generic;
using System.Data;
using PoliNetworkBot_CSharp.Code.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

internal class BanUnbanAllResult
{
    private readonly List<DataRow>? done;
    private readonly List<DataRow>? failed;

    public BanUnbanAllResult(List<DataRow>? done, List<DataRow>? failed)
    {
        this.done = done;
        this.failed = failed;
    }

    internal Language? GetLanguage(RestrictAction ban_true_unban_false, string? target1, long nExceptions)
    {
        var target = "<a href=\"tg://user?id=" + target1 + "\">" + target1 + "</a>";
        switch (ban_true_unban_false)
        {
            case RestrictAction.UNBAN:
            {
                if (done != null)
                    if (failed != null)
                    {
                        var text2 = new Language(new Dictionary<string, string?>
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

                break;
            }

            case RestrictAction.BAN:
            {
                if (done != null)
                    if (failed != null)
                    {
                        var text2 = new Language(new Dictionary<string, string?>
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

                break;
            }

            case RestrictAction.MUTE:
            {
                if (done != null)
                    if (failed != null)
                    {
                        var text2 = new Language(new Dictionary<string, string?>
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

                break;
            }
            case RestrictAction.UNMUTE:
            {
                if (done != null)
                    if (failed != null)
                    {
                        var text2 = new Language(new Dictionary<string, string?>
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

                break;
            }
        }

        return null;
    }

    internal List<DataRow>? GetSuccess()
    {
        return done;
    }

    internal List<DataRow>? GetFailed()
    {
        return failed;
    }
}