using System.Collections.Generic;
using HMS.WebMVC.ViewModels;
using HMS.WebMVC.ViewModels.Pagination;
using HMS.WebMVC.ViewModels.Annotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace HMS.WebMVC.ViewModels
{

    public class CampaignViewModel
    {
        public IEnumerable<CampaignItem> CampaignItems { get; set; }
        public PaginationInfo PaginationInfo { get; set; }

        [LongitudeCoordinate, Required]
        public double Lon { get; set; } = -122.315752;
        [LatitudeCoordinate, Required]
        public double Lat { get; set; } = 47.604610;
    }
}