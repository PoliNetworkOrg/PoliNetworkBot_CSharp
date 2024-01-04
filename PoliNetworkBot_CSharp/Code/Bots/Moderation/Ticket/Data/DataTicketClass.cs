using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Octokit;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Data;

public static class DataTicketClass
{
    private const string NameUserAgent = "PoliNetwork-Ticket";
    public const string OwnerRepo = "PoliNetworkOrg";
    public const string NameRepo = "testIssue";

    public static readonly List<ChatIdTgWith100>
        AllowedGroups = new()
        {
            GroupsConstants.TestGroup,
            GroupsConstants.PianoDiStudi,
            GroupsConstants.Dsu
        };

    private static HttpClient? _httpClient;
    private static GitHubClient? _gitHubClient;

    public static HttpClient GetHttpClient(TelegramBotAbstract telegramBotAbstract)
    {
        if (_httpClient != null)
            return _httpClient;

        _httpClient = new HttpClient();
        var d = _httpClient.DefaultRequestHeaders;
        d.Authorization = new AuthenticationHeaderValue("Bearer", telegramBotAbstract.GithubToken);
        d.Add("Accept", "application/vnd.github+json");
        d.Add("X-GitHub-Api-Version", "2022-11-28");
        d.Add("User-Agent", NameUserAgent);

        return _httpClient;
    }

    public static GitHubClient GetGitHubClient(TelegramBotAbstract telegramBotAbstract)
    {
        if (_gitHubClient != null)
            return _gitHubClient;

        var productHeaderValue = new ProductHeaderValue(NameUserAgent);
        _gitHubClient = new GitHubClient(productHeaderValue);
        var tokenAuth = new Credentials(telegramBotAbstract.GithubToken);
        _gitHubClient.Credentials = tokenAuth;
        return _gitHubClient;
    }
}