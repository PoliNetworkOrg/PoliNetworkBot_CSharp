#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Blacklist;

internal static class Blacklist
{
    private static readonly List<long> SpecialGroups = new() { -1001361547847, -452591994, -1001320704409 };

    private static readonly List<string> BannedWords = new()
    {
        "porcodio", "dioporco", "diocane", "negro", "negri", "negra", "negre"
    };

    internal static async Task<SpamType> IsSpam(string? text, long? groupId, TelegramBotAbstract? telegramBotAbstract,
        bool toLogMistakes, MessageEventArgs? messageEventArgs)
    {
        if (string.IsNullOrEmpty(text))
            return SpamType.ALL_GOOD;

        List<string?> words = new() { text };
        List<string> splitBy = new() { " ", "\"", "'" };
        words = splitBy.Aggregate(words, SplitTextBy);

        var eventArgsContainer = new EventArgsContainer { MessageEventArgs = messageEventArgs };
        if (words is not { Count: > 0 })
            return await CheckNotAllowedWords(text, groupId, telegramBotAbstract, eventArgsContainer) ==
                   SpamType.NOT_ALLOWED_WORDS
                ? SpamType.NOT_ALLOWED_WORDS
                : CheckForFormatMistakes(text, groupId, toLogMistakes);
        var words2 = words.ToList().Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
        if (words2.Any(word => word != null && CheckSpamLink(word, groupId, telegramBotAbstract) == SpamType.SPAM_LINK))
            return SpamType.SPAM_LINK;

        var forwardedFrom = messageEventArgs?.Message.ForwardFrom;
        if (forwardedFrom != null && ForwardBlock.BlockForwardMessageFrom.Contains(forwardedFrom.Id))
            return SpamType.SPAM_LINK;

        return await CheckNotAllowedWords(text, groupId, telegramBotAbstract, eventArgsContainer) ==
               SpamType.NOT_ALLOWED_WORDS
            ? SpamType.NOT_ALLOWED_WORDS
            : CheckForFormatMistakes(text, groupId, toLogMistakes);
    }

    private static List<string?> SplitTextBy(List<string?>? vs, string v)
    {
        if (vs == null)
            return new List<string?>();

        List<string?> r = new();
        foreach (var words in vs.Select(vs2 => vs2?.Split(v).ToList()))
            if (words != null)
                r.AddRange(words);

        return r;
    }

    private static SpamType CheckForFormatMistakes(string? text, long? groupId, bool toLogMistakes)
    {
        var s = CheckForFormatMistakes2(text, groupId);
        return s;
    }

    private static SpamType CheckForFormatMistakes2(string? text, long? groupId)
    {
        if (groupId == null)
            return SpamType.ALL_GOOD;
        var specialGroups = new List<long> { -1001175999519, -1001495422899, -1001164044303 };
        if (specialGroups.All(group => groupId != group))
            return SpamType.ALL_GOOD;
        var textLower = text?.ToLower();
        if (groupId == specialGroups[0])
            return textLower != null && (textLower.Contains("#cerco") || textLower.Contains("#offro") ||
                                         textLower.Contains("#searching") ||
                                         textLower.Contains("#offering"))
                ? SpamType.ALL_GOOD
                : SpamType.FORMAT_INCORRECT;
        if (groupId == specialGroups[1])
            return textLower != null && (textLower.Contains("#richiesta") || textLower.Contains("#offerta"))
                ? SpamType.ALL_GOOD
                : SpamType.FORMAT_INCORRECT;
        if (groupId == specialGroups[2])
            return textLower != null && (textLower.Contains("#cerco") || textLower.Contains("#vendo"))
                ? SpamType.ALL_GOOD
                : SpamType.FORMAT_INCORRECT;
        return SpamType.ALL_GOOD;
    }

    private static async Task<SpamType> CheckNotAllowedWords(string? text, long? groupId,
        TelegramBotAbstract? telegramBotAbstract, EventArgsContainer eventArgsContainer)
    {
        text = text?.ToLower();

        var s = text?.Split(' ');
        if (s != null)
            if (s.Select(RemoveUselessCharacters).Any(s3 => s3 != null && BannedWords.Contains(s3)))
                return SpamType.NOT_ALLOWED_WORDS;


        if (Bitcoin(text, groupId))
            return SpamType.NOT_ALLOWED_WORDS;

        // ReSharper disable once InvertIf
        if (ChiedoScusa(text, groupId))
        {
            await NotifyUtil.NotifyOwnersWithLog(new Exception("Chiedo scusa per lo spam \n\n" + text),
                telegramBotAbstract, null, eventArgsContainer);
            return SpamType.NOT_ALLOWED_WORDS;
        }


        //all good!
        return SpamType.ALL_GOOD;
    }

    private static bool Bitcoin(string? text, long? groupId)
    {
        return text != null && groupId != null && (text.Contains("bitcoin") || text.Contains("bitpanda")) &&
               (text.Contains("guadagn") || text.Contains("rischio")) && SpecialGroups.All(group => groupId != group);
    }

    private static bool ChiedoScusa(string? text, long? groupId)
    {
        return (text != null && groupId != null && (text.Contains("scusate") || text.Contains("chiedo scusa")) &&
                text.Contains("spam")) || (text != null && groupId != null && text.Contains("google") &&
                                           text.Contains("whatsapp") &&
                                           text.Contains("application") && text.Contains("link"));
    }

    private static string? RemoveUselessCharacters(string s3)
    {
        return string.IsNullOrEmpty(s3)
            ? null
            : s3.Where(c => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
                .Aggregate("", (current, c) => current + c);
    }

    private static SpamType CheckSpamLink(string text, long? groupId, TelegramBotAbstract? bot)
    {
        return groupId switch
        {
            -1001307671408 => //gruppo politica
                SpamType.ALL_GOOD,
            _ => CheckSpamLink_DefaultGroup(text, bot)
        };
    }

    private static SpamType CheckSpamLink_DefaultGroup(string text, TelegramBotAbstract? bot)
    {
        if (string.IsNullOrEmpty(text))
            return SpamType.ALL_GOOD;

        if (!text.Contains("t.me/"))
        {
            var b1 = text.Contains("facebook.com") ||
                     text.Contains("whatsapp.com") ||
                     text.Contains("instagram.com") ||
                     text.Contains("bit.ly") ||
                     text.Contains("is.gd") ||
                     text.Contains("amzn.to") ||
                     text.Contains("goo.gl") ||
                     text.Contains("forms.gle") ||
                     text.Contains("docs.google.com") ||
                     text.Contains("amazon.it/gp/student") ||
                     text.Contains("amazon.com/gp/student") ||
                     text.Contains("polinetwork.it") ||
                     text.Contains("discord.gg");
            return b1 ? SpamType.SPAM_LINK : SpamType.ALL_GOOD;
        }

        if (!text.Contains("t.me/"))
            return MustBeTrue(CheckIfIsOurTgLink(text, bot)) ? SpamType.ALL_GOOD : SpamType.SPAM_LINK;
        if (text.Contains("t.me/c/"))
            return SpamType.ALL_GOOD;

        var text1 = text.ToLower();
        var t2 = text1.Split("/");
        var t3 = Find(t2, "t.me");
        if (t3 == null) return MustBeTrue(CheckIfIsOurTgLink(text, bot)) ? SpamType.ALL_GOOD : SpamType.SPAM_LINK;
        var t4 = t2[t3.Value + 1];
        var valid = CheckIfAllowedTag(t4);
        if (valid == SpamType.ALL_GOOD)
            return SpamType.ALL_GOOD;

        return MustBeTrue(CheckIfIsOurTgLink(text, bot)) ? SpamType.ALL_GOOD : SpamType.SPAM_LINK;
    }

    private static bool MustBeTrue(bool? v)
    {
        return v != null && v.Value;
    }

    private static SpamType CheckIfAllowedTag(string t4)
    {
        if (string.IsNullOrEmpty(t4)) return SpamType.ALL_GOOD;

        if (t4.StartsWith("@")) t4 = t4[1..];

        return GlobalVariables.AllowedTags == null
            ? SpamType.SPAM_LINK
            : GlobalVariables.AllowedTags.Contains(t4)
                ? SpamType.ALL_GOOD
                : SpamType.SPAM_LINK;
    }

    private static long? Find(IReadOnlyList<string> t2, string v)
    {
        for (var i = 0; i < t2.Count; i++)
        {
            var t3 = t2[i];
            if (t3 == v)
                return i;
        }

        return null;
    }

    private static bool? CheckIfIsOurTgLink(string text, TelegramBotAbstract? botAbstract)
    {
        const string? q1 = "SELECT id FROM GroupsTelegram WHERE link = @link";
        var link = GetTelegramLink(text);

        if (string.IsNullOrEmpty(link))
            return null;

        link = link.Trim();

        DataTable? dt = null;
        try
        {
            dt = Database.ExecuteSelect(q1, botAbstract?.DbConfig,
                new Dictionary<string, object?> { { "@link", link } });
        }
        catch
        {
            // ignored
        }

        var value = Database.GetFirstValueFromDataTable(dt);
        if (value == null)
            return false;
        var s = value.ToString();
        return !string.IsNullOrEmpty(s);
    }

    private static string? GetTelegramLink(string text)
    {
        var s = text.Contains(' ') ? text.Split(' ') : new[] { text };
        return s.FirstOrDefault(s2 => s2.ToLower().Contains("t.me/"));
    }

    internal static SpamType IsSpam(IEnumerable<PhotoSize>? photo)
    {
        var biggerphoto = UtilsPhoto.GetLargest(photo);

        // ReSharper disable once ConditionalTernaryEqualBranch
        return biggerphoto == null ? SpamType.ALL_GOOD : SpamType.ALL_GOOD; //todo: analizzare la foto con un ocr
    }
}