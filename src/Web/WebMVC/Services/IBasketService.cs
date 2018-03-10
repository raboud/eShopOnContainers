using HMS.WebMVC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.WebMVC.Models;
using HMS.IntegrationEvents;

namespace HMS.WebMVC.Services
{
    public interface IBasketService
    {
        Task<Basket> GetBasket(ApplicationUser user);
        Task AddItemToBasket(ApplicationUser user, BasketItem product);
        Task<Basket> UpdateBasket(Basket basket);
        Task Checkout(BasketDTO basket);
        Task<Basket> SetQuantities(ApplicationUser user, Dictionary<string, int> quantities);
        Order MapBasketToOrder(Basket basket);
    }
}
