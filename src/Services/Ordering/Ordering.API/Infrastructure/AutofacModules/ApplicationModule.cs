using Autofac;
using Microsoft.BuildingBlocks.EventBus.Abstractions;
using HMS.Ordering.API.Application.Commands;
using HMS.Ordering.API.Application.Queries;
using HMS.Ordering.Domain.AggregatesModel.BuyerAggregate;
using HMS.Ordering.Domain.AggregatesModel.OrderAggregate;
using HMS.Ordering.Infrastructure.Idempotency;
using HMS.Ordering.Infrastructure.Repositories;
using System.Reflection;

namespace HMS.Ordering.API.Infrastructure.AutofacModules
{

    public class ApplicationModule
        :Autofac.Module
    {

        public string QueriesConnectionString { get; }

        public ApplicationModule(string qconstr)
        {
            QueriesConnectionString = qconstr;

        }

        protected override void Load(ContainerBuilder builder)
        {

            builder.Register(c => new OrderQueries(QueriesConnectionString))
                .As<IOrderQueries>()
                .InstancePerLifetimeScope();

            builder.RegisterType<BuyerRepository>()
                .As<IBuyerRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<OrderRepository>()
                .As<IOrderRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<RequestManager>()
               .As<IRequestManager>()
               .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(CreateOrderCommandHandler).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(IIntegrationEventHandler<>));

        }
    }
}
