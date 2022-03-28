﻿#region

using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace Minista.Helpers;

public class UserAgentHelper
{
    private const int URLMON_OPTION_USERAGENT = 0x10000001;

    [DllImport("urlmon.dll", CharSet = CharSet.Unicode)]
    private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength,
        int dwReserved);

    [DllImport("urlmon.dll", CharSet = CharSet.Unicode)]
    private static extern int UrlMkGetSessionOption(int dwOption, StringBuilder pBuffer, int dwBufferLength,
        ref int pdwBufferLength, int dwReserved);

    private static string GetUserAgent()
    {
        const int capacity = 255;
        var buf = new StringBuilder(capacity);
        var length = 0;

        _ = UrlMkGetSessionOption(URLMON_OPTION_USERAGENT, buf, capacity, ref length, 0);

        return buf.ToString();
    }

    public static void SetUserAgent(string agent)
    {
        var hr = UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, agent, agent.Length, 0);
        var ex = Marshal.GetExceptionForHR(hr);
        if (null != ex) throw ex;
    }

    public static void AppendUserAgent(string suffix)
    {
        SetUserAgent(GetUserAgent() + suffix);
    }

    /*[DllImport("urlmon.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);

    private const int URLMON_OPTION_USERAGENT = 0x10000001;

    public static void SetDefaultUserAgent(string userAgent)
    {
        UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, userAgent, userAgent.Length, 0);
    }*/
}