using HMS.WebMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HMS.WebMVC.Services;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HMS.IntegrationEvents;

namespace HMS.WebMVC.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IBasketService _basketSvc;
        private readonly ICatalogService _catalogSvc;
        private readonly IIdentityParser<ApplicationUser> _appUserParser;

        public CartController(IBasketService basketSvc, ICatalogService catalogSvc, IIdentityParser<ApplicationUser> appUserParser)
        {
            _basketSvc = basketSvc;
            _catalogSvc = catalogSvc;
            _appUserParser = appUserParser;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
				ApplicationUser user = _appUserParser.Parse(HttpContext.User);
				Basket vm = await _basketSvc.GetBasket(user);

                return View(vm);
            }
            catch (BrokenCircuitException)
            {
                // Catch error when Basket.api is in circuit-opened mode                 
                HandleBrokenCircuitException();
            }

            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Index(Dictionary<string, int> quantities, string action)
        {
            try
            {
				ApplicationUser user = _appUserParser.Parse(HttpContext.User);
				Basket basket = await _basketSvc.SetQuantities(user, quantities);
				Basket vm = await _basketSvc.UpdateBasket(basket);

                if (action == "[ Checkout ]")
                {
					Order order = _basketSvc.MapBasketToOrder(basket);
                    return RedirectToAction("Create", "Order");
                }
            }
            catch (BrokenCircuitException)
            {
                // Catch error when Basket.api is in circuit-opened mode                 
                HandleBrokenCircuitException();
            }

            return View();
        }

        public async Task<IActionResult> AddToCart(CatalogItem productDetails)
        {
            try
            {
                if (productDetails.Id != null)
                {
					ApplicationUser user = _appUserParser.Parse(HttpContext.User);
					BasketItem product = new BasketItem()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Quantity = 1,
                        ProductName = productDetails.Name,
                        PictureUrl = productDetails.PictureUri,
                        UnitPrice = productDetails.Price,
                        ProductId = productDetails.Id
                    };
                    await _basketSvc.AddItemToBasket(user, product);
                }
                return RedirectToAction("Index", "Catalog");            
            }
            catch (BrokenCircuitException)
            {
                // Catch error when Basket.api is in circuit-opened mode                 
                HandleBrokenCircuitException();
            }

            return RedirectToAction("Index", "Catalog", new { errorMsg = ViewBag.BasketInoperativeMsg });
        }

        private void HandleBrokenCircuitException()
        {
            ViewBag.BasketInoperativeMsg = "Basket Service is inoperative, please try later on. (Business Msg Due to Circuit-Breaker)";
        }
    }
}
