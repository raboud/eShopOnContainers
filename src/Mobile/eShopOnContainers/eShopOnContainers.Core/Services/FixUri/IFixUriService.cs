using System.Collections.Generic;
using HMS.Core.Models.Basket;
using HMS.Core.Models.Catalog;
using HMS.Core.Models.Marketing;

namespace HMS.Core.Services.FixUri
{
    public interface IFixUriService
    {
        void FixCatalogItemPictureUri(IEnumerable<CatalogItem> catalogItems);
        void FixBasketItemPictureUri(IEnumerable<BasketItem> basketItems);
        void FixCampaignItemPictureUri(IEnumerable<CampaignItem> campaignItems);
    }
}
