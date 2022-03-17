﻿#region

using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.Logger;
using System;
using System.Net.Http;
using System.Threading.Tasks;

#endregion

namespace InstagramApiSharp.Classes
{
    internal class HttpRequestProcessor : IHttpRequestProcessor
    {
        private readonly IInstaLogger _logger;

        public HttpRequestProcessor(IRequestDelay delay, HttpClient httpClient, HttpClientHandler httpHandler,
            ApiRequestMessage requestMessage, IInstaLogger logger)
        {
            Delay = delay;
            Client = httpClient;
            HttpHandler = httpHandler;
            RequestMessage = requestMessage;
            _logger = logger;
        }

        public IRequestDelay Delay { get; set; }

        public HttpClientHandler HttpHandler { get; set; }
        public ApiRequestMessage RequestMessage { get; }
        public HttpClient Client { get; set; }

        public void SetHttpClientHandler(HttpClientHandler handler)
        {
            HttpHandler = handler;
            Client = new HttpClient(handler);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage)
        {
            Client.DefaultRequestHeaders.ConnectionClose = false;
            requestMessage.Headers.Add("Connection", "Keep-Alive");
            LogHttpRequest(requestMessage);
            if (Delay.Exist)
                await Task.Delay(Delay.Value);
            var response = await Client.SendAsync(requestMessage);
            LogHttpResponse(response);
            return response;
        }

        public async Task<HttpResponseMessage> GetAsync(Uri requestUri)
        {
            Client.DefaultRequestHeaders.ConnectionClose = false;
            _logger?.LogRequest(requestUri);
            if (Delay.Exist)
                await Task.Delay(Delay.Value);
            var response = await Client.GetAsync(requestUri);
            LogHttpResponse(response);
            return response;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage,
            HttpCompletionOption completionOption)
        {
            Client.DefaultRequestHeaders.ConnectionClose = false;
            requestMessage.Headers.Add("Connection", "Keep-Alive");
            LogHttpRequest(requestMessage);
            if (Delay.Exist)
                await Task.Delay(Delay.Value);
            var response = await Client.SendAsync(requestMessage, completionOption);
            LogHttpResponse(response);
            return response;
        }

        public async Task<string> SendAndGetJsonAsync(HttpRequestMessage requestMessage,
            HttpCompletionOption completionOption)
        {
            Client.DefaultRequestHeaders.ConnectionClose = false;
            LogHttpRequest(requestMessage);
            if (Delay.Exist)
                await Task.Delay(Delay.Value);
            var response = await Client.SendAsync(requestMessage, completionOption);
            LogHttpResponse(response);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GeJsonAsync(Uri requestUri)
        {
            Client.DefaultRequestHeaders.ConnectionClose = false;
            _logger?.LogRequest(requestUri);
            if (Delay.Exist)
                await Task.Delay(Delay.Value);
            var response = await Client.GetAsync(requestUri);
            LogHttpResponse(response);
            return await response.Content.ReadAsStringAsync();
        }

        private void LogHttpRequest(HttpRequestMessage request)
        {
            _logger?.LogRequest(request);
        }

        private void LogHttpResponse(HttpResponseMessage request)
        {
            _logger?.LogResponse(request);
        }
    }
}