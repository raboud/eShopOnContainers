using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HMS.WebMVC.Controllers;
using HMS.WebMVC.Services;
using HMS.WebMVC.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using BasketModel = HMS.WebMVC.ViewModels.Basket;
using HMS.IntegrationEvents;

namespace HMS.UnitTest.Basket.Application
{
    public class CartControllerTest
    {
        private readonly Mock<ICatalogService> _catalogServiceMock;
        private readonly Mock<IBasketService> _basketServiceMock;
        private readonly Mock<IIdentityParser<ApplicationUser>> _identityParserMock;
        private readonly Mock<HttpContext> _contextMock;

        public CartControllerTest()
        {
            _catalogServiceMock = new Mock<ICatalogService>();
            _basketServiceMock = new Mock<IBasketService>();
            _identityParserMock = new Mock<IIdentityParser<ApplicationUser>>();
            _contextMock = new Mock<HttpContext>();
        }

        [Fact]
        public async Task Post_cart_success()
        {
			//Arrange
			string fakeBuyerId = "1";
			string action = string.Empty;
			BasketModel fakeBasket = GetFakeBasket(fakeBuyerId);
			Dictionary<string, int> fakeQuantities = new Dictionary<string, int>()
            {
                ["fakeProdA"] = 1,
                ["fakeProdB"] = 2
            };

            _basketServiceMock.Setup(x => x.SetQuantities(It.IsAny<ApplicationUser>(), It.IsAny<Dictionary<string, int>>()))
                .Returns(Task.FromResult(fakeBasket));

            _basketServiceMock.Setup(x => x.UpdateBasket(It.IsAny<BasketModel>()))
                .Returns(Task.FromResult(fakeBasket));

			//Act
			CartController cartController = new CartController(_basketServiceMock.Object, _catalogServiceMock.Object, _identityParserMock.Object);
            cartController.ControllerContext.HttpContext = _contextMock.Object;
			IActionResult actionResult = await cartController.Index(fakeQuantities, action);

			//Assert
			ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        }

        [Fact]
        public async Task Post_cart_checkout_success()
        {
			//Arrange
			string fakeBuyerId = "1";
			string action = "[ Checkout ]";
			BasketModel fakeBasket = GetFakeBasket(fakeBuyerId);
			Dictionary<string, int> fakeQuantities = new Dictionary<string, int>()
            {
                ["fakeProdA"] = 1,
                ["fakeProdB"] = 2
            };

            _basketServiceMock.Setup(x => x.SetQuantities(It.IsAny<ApplicationUser>(), It.IsAny<Dictionary<string, int>>()))
                .Returns(Task.FromResult(fakeBasket));

            _basketServiceMock.Setup(x => x.UpdateBasket(It.IsAny<BasketModel>()))
                .Returns(Task.FromResult(fakeBasket));

			//Act
			CartController orderController = new CartController(_basketServiceMock.Object, _catalogServiceMock.Object, _identityParserMock.Object);
            orderController.ControllerContext.HttpContext = _contextMock.Object;
			IActionResult actionResult = await orderController.Index(fakeQuantities, action);

			//Assert
			RedirectToActionResult redirectToActionResult = Assert.IsType<RedirectToActionResult>(actionResult);
            Assert.Equal("Order", redirectToActionResult.ControllerName);
            Assert.Equal("Create", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Add_to_cart_success()
        {
			//Arrange
			CatalogItem fakeCatalogItem = GetFakeCatalogItem();

            _basketServiceMock.Setup(x => x.AddItemToBasket(It.IsAny<ApplicationUser>(), It.IsAny<BasketItem>()))
                .Returns(Task.FromResult(1));

			//Act
			CartController orderController = new CartController(_basketServiceMock.Object, _catalogServiceMock.Object, _identityParserMock.Object);
            orderController.ControllerContext.HttpContext = _contextMock.Object;
			IActionResult actionResult = await orderController.AddToCart(fakeCatalogItem);

			//Assert
			RedirectToActionResult redirectToActionResult = Assert.IsType<RedirectToActionResult>(actionResult);
            Assert.Equal("Catalog", redirectToActionResult.ControllerName);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        private BasketModel GetFakeBasket(string buyerId)
        {
            return new BasketModel()
            {
                BuyerId = buyerId
            };
        }

        private CatalogItem GetFakeCatalogItem()
        {
            return new CatalogItem()
            {
                Id = "1",
                Name = "fakeName",
                CatalogBrand = "fakeBrand",
                CatalogType = "fakeType",
                CatalogBrandId = 2,
                CatalogTypeId = 5,
                Price = 20
            };
        }
    }
}
