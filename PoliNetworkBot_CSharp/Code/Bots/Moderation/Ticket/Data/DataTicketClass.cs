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
    public const int MaxLengthTitleIssue = 200;
    public const int MaxTimeThreadInRamDays = 3;

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
        SetTokenToHttpClient(telegramBotAbstract.GithubToken, _httpClient);
        var d = _httpClient.DefaultRequestHeaders;
        d.Add("Accept", "application/vnd.github+json");
        d.Add("X-GitHub-Api-Version", "2022-11-28");
        d.Add("User-Agent", NameUserAgent);

        return _httpClient;
    }

    private static void SetTokenToHttpClient(string? token, HttpClient httpClient2)
    {
        var d = httpClient2.DefaultRequestHeaders;
        d.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static GitHubClient? GetGitHubClient(TelegramBotAbstract telegramBotAbstract)
    {
        if (_gitHubClient != null)
            return _gitHubClient;

        if (string.IsNullOrEmpty(telegramBotAbstract.GithubToken))
            return null;

        var productHeaderValue = new ProductHeaderValue(NameUserAgent);
        _gitHubClient = new GitHubClient(productHeaderValue);
        SetGithubTokenCredentialsClient(telegramBotAbstract.GithubToken, _gitHubClient);
        return _gitHubClient;
    }

    private static void SetGithubTokenCredentialsClient(string? newToken, GitHubClient gitHubClient)
    {
        var tokenAuth = new Credentials(newToken);
        gitHubClient.Credentials = tokenAuth;
    }

    public static void SetToken(string? githubToken)
    {
        if (_httpClient != null)
            SetTokenToHttpClient(githubToken, _httpClient);

        if (_gitHubClient != null)
            SetGithubTokenCredentialsClient(githubToken, _gitHubClient);
    }
}