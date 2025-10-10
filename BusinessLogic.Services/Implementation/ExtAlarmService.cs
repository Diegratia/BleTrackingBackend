using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic.Services.Implementation
{
    public class ExtAlarmService : IExtAlarmService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExtAlarmService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task NotifyFaceMatchedAsync(string entityName)
        {
            var baseUrl = _configuration["HcpSettings:BaseUrl"];
            var endpoint = $"{baseUrl}/api/eventService/v1/eventSubscriptionByEventTypes";
            var eventTypes = _configuration.GetSection($"HcpSettings:EventTypes:{entityName}").Get<int[]>() ?? new[] { 131659 };
            // Dynamically construct eventDest using the current request's scheme and host
          // Dynamically construct eventDest using the current request's scheme and host
            string eventDest;
            if (_httpContextAccessor.HttpContext != null)
            {
                var request = _httpContextAccessor.HttpContext.Request;
                eventDest = $"{request.Scheme}://{request.Host}/test/hit";
            }
            else
            {
                // Fallback to appsettings.json if no HTTP context is available
                eventDest = _configuration["HcpSettings:EventDest"] ?? "http://localhost:5000/test/hit";
            }

            // Set custom headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-ca-key", _configuration["HcpSettings:x-ca-key"]);
            _httpClient.DefaultRequestHeaders.Add("x-ca-signature", _configuration["HcpSettings:x-ca-signature"]);
            _httpClient.DefaultRequestHeaders.Add("x-ca-signature-headers", _configuration["HcpSettings:x-ca-signature-headers"]);
            _httpClient.DefaultRequestHeaders.Add("x-ca-nonce", _configuration["HcpSettings:x-ca-nonce"]);

            var subscriptionRequest = new EventSubscriptionRequest
            {
                EventTypes = eventTypes,
                EventDest = eventDest
            };

            var json = JsonSerializer.Serialize(subscriptionRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to subscribe to HCP events: {response.ReasonPhrase}");
            }
        }
    }
}