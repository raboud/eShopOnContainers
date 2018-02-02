using HMS.Core.Models.Marketing;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HMS.Core.Services.Marketing
{
    public interface ICampaignService
    {
        Task<ObservableCollection<CampaignItem>> GetAllCampaignsAsync(string token);
        Task<CampaignItem> GetCampaignByIdAsync(int id, string token);
    }
}