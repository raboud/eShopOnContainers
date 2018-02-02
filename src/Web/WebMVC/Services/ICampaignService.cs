using HMS.Common.API;
using HMS.WebMVC.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HMS.WebMVC.Services
{
    public interface ICampaignService
    {
        Task<PaginatedItemsViewModel<CampaignItem>> GetCampaigns(int pageSize, int pageIndex);

        Task<CampaignItem> GetCampaignById(int id);
    }
}