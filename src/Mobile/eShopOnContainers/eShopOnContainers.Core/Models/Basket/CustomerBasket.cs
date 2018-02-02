using System.Collections.Generic;

namespace HMS.Core.Models.Basket
{
    public class CustomerBasket
    {
        public string BuyerId { get; set; }
        public List<BasketItem> Items { get; set; }
    }
}
