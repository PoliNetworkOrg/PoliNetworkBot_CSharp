using System.Net.Http;
using Octokit;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket;

public static class Data
{
    private const string NameUserAgent = "PoliNetwork-Ticket";
    public const string OwnerRepo = "PoliNetworkOrg";
    public const string NameRepo = "testIssue";
    
    private static HttpClient? _httpClient;
    private static GitHubClient? _gitHubClient;

    public static HttpClient GetHttpClient(TelegramBotAbstract telegramBotAbstract)
    {
        if (_httpClient != null)
            return _httpClient;
        
        _httpClient = new HttpClient();
        var d = _httpClient.DefaultRequestHeaders;
        d.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", telegramBotAbstract.GithubToken);
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