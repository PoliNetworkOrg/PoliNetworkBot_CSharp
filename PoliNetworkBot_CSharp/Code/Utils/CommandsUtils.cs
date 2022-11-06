using System;
using System.Collections.Generic;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Utils;

public class CommandsUtils
{
    /// <summary>
    ///     body returns everything except the tags
    /// </summary>
    /// <param name="helpMessage"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    private static Language ParseText(Language helpMessage, string tag)
    {
        var toReturn = new Dictionary<string, string?>();
        foreach (var language in helpMessage.GetLanguages())
        {
            var select = helpMessage.Select(language) ?? "";
            if (tag == "body")
            {
                toReturn.Add(language, select.Split(" @")[0].Replace("/@", "@"));
            }
            else
            {
                var tags = select.Split(" @").Skip(1).ToList();
                var innerTags = tags.Where(innerTag => innerTag.Split(": ")[0] == tag)
                    .Select(x => x.Replace(tag + ": ", "").Replace("/@", "@"));

                foreach (var innerTag in innerTags) toReturn.Add(language, innerTag);
            }
        }

        return new Language(toReturn);
    }

    private static string MessageParser(Language helpMessage, List<string> trigger, string lang)
    {
        var body = ParseText(helpMessage, "body");
        var args = ParseText(helpMessage, "args");
        var condition = ParseText(helpMessage, "condition");
        var toReturn = "/<b>" + string.Join(" | /", trigger.ToArray()) + "</b>:\n" + body.Select(lang);

        if (!string.IsNullOrEmpty(args.Select(lang)))
            toReturn += "<i>\nArguments: </i>" + args.Select(lang);

        if (!string.IsNullOrEmpty(condition.Select(lang)))
            toReturn += "<i>\nConditions: </i>" + condition.Select(lang) + "";

        return toReturn;
    }

    public static Language GenerateMessage(Language messageToParse, Permission clearance, Permission permissionLevel,
        List<string> trigger)
    {
        var languages = new Dictionary<string, string?>();
        if (Permissions.Compare(clearance, permissionLevel) < 0)
            return new Language(languages);

        foreach (var lang in messageToParse.GetLanguages())
        {
            var text = MessageParser(messageToParse, trigger, lang);

            languages.Add(lang, text + "\n\n");
        }

        return new Language(languages);
    }
}

public enum CommandExecutionState
{
    SUCCESSFUL, NOT_TRIGGERED, UNMET_CONDITIONS, INSUFFICIENT_PERMISSIONS, ERROR_NOT_ENABLED
}
