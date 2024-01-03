﻿using System;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket;

[Serializable]
public class ChatIdTgWith100
{
    public long Id;
    public bool VaAggiuntoMeno100;
    public string? Category;

    public string GetString()
    {
        return VaAggiuntoMeno100 ? "-100" + Id : Id.ToString();
    }

    public long FullLong()
    {
        var value = GetString();
        return Convert.ToInt64(value);
    }
}