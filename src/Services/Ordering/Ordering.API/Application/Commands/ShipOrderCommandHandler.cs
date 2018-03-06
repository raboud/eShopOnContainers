﻿using MediatR;
using HMS.Ordering.API.Application.Commands;
using HMS.Ordering.Domain.AggregatesModel.OrderAggregate;
using HMS.Ordering.Infrastructure.Idempotency;
using System.Threading;
using System.Threading.Tasks;

namespace HMS.Ordering.API.Application.Commands
{
    // Regular CommandHandler
    public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand, bool>
    {        
        private readonly IOrderRepository _orderRepository;

        public ShipOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Handler which processes the command when
        /// administrator executes ship order from app
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<bool> Handle(ShipOrderCommand command, CancellationToken cancellationToken)
        {
			Order orderToUpdate = await _orderRepository.GetAsync(command.OrderNumber);
            if(orderToUpdate == null)
            {
                return false;
            }

            orderToUpdate.SetShippedStatus();
            return await _orderRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }


    // Use for Idempotency in Command process
    public class ShipOrderIdentifiedCommandHandler : IdentifiedCommandHandler<ShipOrderCommand, bool>
    {
        public ShipOrderIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override bool CreateResultForDuplicateRequest()
        {
            return true;                // Ignore duplicate requests for processing order.
        }
    }
}
