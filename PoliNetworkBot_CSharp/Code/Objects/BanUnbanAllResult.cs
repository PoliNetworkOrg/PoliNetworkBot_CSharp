#region

using System.Collections.Generic;
using System.Data;
using PoliNetworkBot_CSharp.Code.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

internal class BanUnbanAllResult
{
    private readonly List<DataRow>? _done;
    private readonly List<DataRow>? _failed;

    public BanUnbanAllResult(List<DataRow>? done, List<DataRow>? failed)
    {
        _done = done;
        _failed = failed;
    }

    internal Language? GetLanguage(RestrictAction banTrueUnbanFalse, TargetUserObject target1, long nExceptions)
    {
        var target = target1.GetTargetHtmlString();
        switch (banTrueUnbanFalse)
        {
            case RestrictAction.UNBAN:
            {
                if (_done != null)
                    if (_failed != null)
                    {
                        var text2 = new Language(new Dictionary<string, string?>
                        {
                            {
                                "en",
                                "Target " + target + " unbanned from " + _done.Count + " groups" + "\n" +
                                "not unbanned from " + _failed.Count + " groups" + "\n" +
                                "exception number = " + nExceptions
                            },

                            {
                                "it",
                                "Target " + target + " sbannato da " + _done.Count + " gruppi" + "\n" +
                                "non sbannato da " + _failed.Count + " gruppi" + "\n" +
                                "numero eccezioni = " + nExceptions
                            }
                        });

                        return text2;
                    }

                break;
            }

            case RestrictAction.BAN:
            {
                if (_done != null)
                    if (_failed != null)
                    {
                        var text2 = new Language(new Dictionary<string, string?>
                        {
                            {
                                "en",
                                "Target " + target + " banned from " + _done.Count + " groups" + "\n" +
                                "not banned from " + _failed.Count + " groups" + "\n" +
                                "exception number = " + nExceptions
                            },

                            {
                                "it",
                                "Target " + target + " bannato da " + _done.Count + " gruppi" + "\n" +
                                "non bannato da " + _failed.Count + " gruppi" + "\n" +
                                "numero eccezioni = " + nExceptions
                            }
                        });

                        return text2;
                    }

                break;
            }

            case RestrictAction.MUTE:
            {
                if (_done != null)
                    if (_failed != null)
                    {
                        var text2 = new Language(new Dictionary<string, string?>
                        {
                            {
                                "en",
                                "Target " + target + " muted from " + _done.Count + " groups" + "\n" +
                                "not muted from " + _failed.Count + " groups" + "\n" +
                                "exception number = " + nExceptions
                            },

                            {
                                "it",
                                "Target " + target + " mutato da " + _done.Count + " gruppi" + "\n" +
                                "non mutato da " + _failed.Count + " gruppi" + "\n" +
                                "numero eccezioni = " + nExceptions
                            }
                        });

                        return text2;
                    }

                break;
            }
            case RestrictAction.UNMUTE:
            {
                if (_done != null)
                    if (_failed != null)
                    {
                        var text2 = new Language(new Dictionary<string, string?>
                        {
                            {
                                "en",
                                "Target " + target + " unmuted from " + _done.Count + " groups" + "\n" +
                                "not unmuted from " + _failed.Count + " groups" + "\n" +
                                "exception number = " + nExceptions
                            },

                            {
                                "it",
                                "Target " + target + " smutato da " + _done.Count + " gruppi" + "\n" +
                                "non smutato da " + _failed.Count + " gruppi" + "\n" +
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
        return _done;
    }

    internal List<DataRow>? GetFailed()
    {
        return _failed;
    }
}