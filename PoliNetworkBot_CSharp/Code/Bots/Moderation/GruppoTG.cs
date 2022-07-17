#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation;

public class GruppoTg
{
    private readonly DateTime? _lastUpdateInviteLinkTimeDateTime;
    public readonly string? Nome;
    public readonly List<string?> OldLinks;
    public string? IdLink;
    public string? NewLink;
    public long? PermanentId;

    public GruppoTg(JToken? idLink, JToken? nome, JToken? id, JToken? lastUpdateInviteLinkTime)
    {
        IdLink = idLink?.ToString();
        OldLinks = new List<string?> { IdLink };
        Nome = nome?.ToString();

        try
        {
            PermanentId = Convert.ToInt64(id?.ToString());
        }
        catch
        {
            // ignored
        }

        try
        {
            var lastUpdateInviteLinkTimeString = lastUpdateInviteLinkTime?.ToString();
            if (lastUpdateInviteLinkTimeString != null && !lastUpdateInviteLinkTimeString.Contains(' ')) return;
            var s1 = lastUpdateInviteLinkTimeString?.Split(' ');
            if (s1 == null) return;
            var s2 = s1[0]; //2021-06-30
            var s3 = s1[1]; //22:12:06.399

            if (!s2.Contains('-')) return;
            var s4 = s2.Split('-');
            var year = s4[0];
            var month = s4[1];
            var day = s4[2];

            if (!s3.Contains('.')) return;
            var s5 = s3.Split(".");
            var s6 = s5[0]; //22:12:06
            var millisecond = s5[1];

            if (!s6.Contains(':')) return;
            var s7 = s6.Split(':');

            var hour = s7[0];
            var minute = s7[1];
            var second = s7[2];

            _lastUpdateInviteLinkTimeDateTime = new DateTime(
                (int)Convert.ToInt64(year),
                (int)Convert.ToInt64(month),
                (int)Convert.ToInt64(day),
                (int)Convert.ToInt64(hour),
                (int)Convert.ToInt64(minute),
                (int)Convert.ToInt64(second),
                (int)Convert.ToInt64(millisecond));
        }
        catch
        {
            // ignored
        }
    }

    internal void UpdateId(long value)
    {
        PermanentId = value;
    }

    internal void UpdateNewLink(string? link)
    {
        NewLink = link;
    }
}