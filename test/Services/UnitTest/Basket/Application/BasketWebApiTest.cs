using HMS.Basket.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.BuildingBlocks.EventBus.Abstractions;
using HMS.Basket.API.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using IBasketIdentityService = HMS.Basket.API.Services.IIdentityService;
using HMS.IntegrationEvents.Events;
using HMS.IntegrationEvents;

namespace HMS.UnitTest.Basket.Application
{
    public class BasketWebApiTest
    {
        private readonly Mock<IBasketRepository> _basketRepositoryMock;
        private readonly Mock<IBasketIdentityService> _identityServiceMock;
        private readonly Mock<IEventBus> _serviceBusMock;

        public BasketWebApiTest()
        {
            _basketRepositoryMock = new Mock<IBasketRepository>();
            _identityServiceMock = new Mock<IBasketIdentityService>();
            _serviceBusMock = new Mock<IEventBus>();
        }

        [Fact]
        public async Task Get_customer_basket_success()
        {
			//Arrange
			string fakeCustomerId = "1";
			CustomerBasket fakeCustomerBasket = GetCustomerBasketFake(fakeCustomerId);

            _basketRepositoryMock.Setup(x => x.GetBasketAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(fakeCustomerBasket));
            _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(fakeCustomerId);

            _serviceBusMock.Setup(x => x.Publish(It.IsAny<UserCheckoutAcceptedIntegrationEvent>()));
			//Act
			BasketController basketController = new BasketController(
                _basketRepositoryMock.Object, _identityServiceMock.Object, _serviceBusMock.Object);
			OkObjectResult actionResult = await basketController.Get(fakeCustomerId) as OkObjectResult;

            //Assert
            Assert.Equal(actionResult.StatusCode, (int)System.Net.HttpStatusCode.OK);
            Assert.Equal(((CustomerBasket)actionResult.Value).BuyerId, fakeCustomerId);
        }

        [Fact]
        public async Task Post_customer_basket_success()
        {
			//Arrange
			string fakeCustomerId = "1";
			CustomerBasket fakeCustomerBasket = GetCustomerBasketFake(fakeCustomerId);

            _basketRepositoryMock.Setup(x => x.UpdateBasketAsync(It.IsAny<CustomerBasket>()))
                .Returns(Task.FromResult(fakeCustomerBasket));
            _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(fakeCustomerId);
            _serviceBusMock.Setup(x => x.Publish(It.IsAny<UserCheckoutAcceptedIntegrationEvent>()));
			//Act
			BasketController basketController = new BasketController(
                _basketRepositoryMock.Object, _identityServiceMock.Object, _serviceBusMock.Object);

			OkObjectResult actionResult = await basketController.Post(fakeCustomerBasket) as OkObjectResult;

            //Assert
            Assert.Equal(actionResult.StatusCode, (int)System.Net.HttpStatusCode.OK);
            Assert.Equal(((CustomerBasket)actionResult.Value).BuyerId, fakeCustomerId);
        }        

        [Fact]
        public async Task Doing_Checkout_Without_Basket_Should_Return_Bad_Request()
        {
			string fakeCustomerId = "2";
            _basketRepositoryMock.Setup(x => x.GetBasketAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((CustomerBasket)null));
            _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(fakeCustomerId);
			//Act
			BasketController basketController = new BasketController(
                _basketRepositoryMock.Object, _identityServiceMock.Object, _serviceBusMock.Object);

			BadRequestResult result = await basketController.Checkout(new BasketCheckout(), Guid.NewGuid().ToString()) as BadRequestResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Doing_Checkout_Wit_Basket_Should_Publish_UserCheckoutAccepted_Integration_Event()
        {
			string fakeCustomerId = "1";
			CustomerBasket fakeCustomerBasket = GetCustomerBasketFake(fakeCustomerId);
            _basketRepositoryMock.Setup(x => x.GetBasketAsync(It.IsAny<string>()))
                 .Returns(Task.FromResult(fakeCustomerBasket));
            _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(fakeCustomerId);
			//Act
			BasketController basketController = new BasketController(
                _basketRepositoryMock.Object, _identityServiceMock.Object, _serviceBusMock.Object);

			AcceptedResult result = await basketController.Checkout(new BasketCheckout(), Guid.NewGuid().ToString()) as AcceptedResult;
            _serviceBusMock.Verify(mock => mock.Publish(It.IsAny<UserCheckoutAcceptedIntegrationEvent>()), Times.Once);
            Assert.NotNull(result);
        }

        private CustomerBasket GetCustomerBasketFake(string fakeCustomerId)
        {            
            return new CustomerBasket(fakeCustomerId)
            {
                Items = new List<BasketItem>()
                {
                    new BasketItem()
                }
            };
        }
    }
}
