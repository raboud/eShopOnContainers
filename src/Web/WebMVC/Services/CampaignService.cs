using HMS.WebMVC.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.BuildingBlocks.Resilience.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using HMS.Common.API;
using HMS.WebMVC.ViewModels;

namespace HMS.WebMVC.Services
{
	public class CampaignService : ICampaignService
    {
        private readonly IOptionsSnapshot<AppSettings> _settings;
        private readonly IHttpClient _apiClient;
        private readonly ILogger<CampaignService> _logger;
        private readonly string _remoteServiceBaseUrl;
        private readonly IHttpContextAccessor _httpContextAccesor;

        public CampaignService(IOptionsSnapshot<AppSettings> settings, IHttpClient httpClient,
            ILogger<CampaignService> logger, IHttpContextAccessor httpContextAccesor)
        {
            _settings = settings;
            _apiClient = httpClient;
            _logger = logger;

            _remoteServiceBaseUrl = $"{_settings.Value.MarketingUrl}/api/v1/campaigns/";
            _httpContextAccesor = httpContextAccesor ?? throw new ArgumentNullException(nameof(httpContextAccesor));
        }

        public async Task<PaginatedItemsViewModel<CampaignItem>> GetCampaigns(int pageSize, int pageIndex)
        {
            var allCampaignItemsUri = API.Marketing.GetAllCampaigns(_remoteServiceBaseUrl, 
                pageSize, pageIndex);

            var authorizationToken = await GetUserTokenAsync();
            var dataString = await _apiClient.GetStringAsync(allCampaignItemsUri, authorizationToken);

            var response = JsonConvert.DeserializeObject<PaginatedItemsViewModel<CampaignItem>>(dataString);

            return response;
        }

        public async Task<CampaignItem> GetCampaignById(int id)
        {
            var campaignByIdItemUri = API.Marketing.GetAllCampaignById(_remoteServiceBaseUrl, id);

            var authorizationToken = await GetUserTokenAsync();
            var dataString = await _apiClient.GetStringAsync(campaignByIdItemUri, authorizationToken);

            var response = JsonConvert.DeserializeObject<CampaignItem>(dataString);

            return response;
        }

        private string GetUserIdentity()
        {
            return _httpContextAccesor.HttpContext.User.FindFirst("sub").Value;
        }

        private async Task<string> GetUserTokenAsync()
        {
            var context = _httpContextAccesor.HttpContext;
            return await context.GetTokenAsync("access_token");
        }
    }
}