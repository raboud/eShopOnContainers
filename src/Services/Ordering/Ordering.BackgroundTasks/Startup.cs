using Autofac;
using Autofac.Extensions.DependencyInjection;
using HMS.Ordering.BackgroundTasks.Configuration;
using HMS.Ordering.BackgroundTasks.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.ServiceBus;
using Microsoft.BuildingBlocks.EventBus;
using Microsoft.BuildingBlocks.EventBus.Abstractions;
using Microsoft.BuildingBlocks.EventBusRabbitMQ;
using Microsoft.BuildingBlocks.EventBusServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;

namespace HMS.Ordering.BackgroundTasks
{
	public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //add health check for this service
            services.AddHealthChecks(checks =>
            {
				int minutes = 1;

                if (int.TryParse(Configuration["HealthCheck:Timeout"], out int minutesParsed))
                {
                    minutes = minutesParsed;
                }
                checks.AddSqlCheck("OrderingDb", Configuration["ConnectionString"], TimeSpan.FromMinutes(minutes));
            });

            //configure settings

            services.Configure<BackgroundTaskSettings>(Configuration);
            services.AddOptions();

            //configure background task

            services.AddSingleton<IHostedService, GracePeriodManagerService>();

            //configure event bus related services

            if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddSingleton<IServiceBusPersisterConnection>(sp =>
                {
					ILogger<DefaultServiceBusPersisterConnection> logger = sp.GetRequiredService<ILogger<DefaultServiceBusPersisterConnection>>();

					string serviceBusConnectionString = Configuration["EventBusConnection"];
					ServiceBusConnectionStringBuilder serviceBusConnection = new ServiceBusConnectionStringBuilder(serviceBusConnectionString);

                    return new DefaultServiceBusPersisterConnection(serviceBusConnection, logger);
                });
            }
            else
            {
                services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
                {
					ILogger<DefaultRabbitMQPersistentConnection> logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();


					ConnectionFactory factory = new ConnectionFactory()
                    {
                        HostName = Configuration["EventBusConnection"]
                    };

                    if (!string.IsNullOrEmpty(Configuration["EventBusUserName"]))
                    {
                        factory.UserName = Configuration["EventBusUserName"];
                    }

                    if (!string.IsNullOrEmpty(Configuration["EventBusPassword"]))
                    {
                        factory.Password = Configuration["EventBusPassword"];
                    }

					int retryCount = 5;
                    if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
                    {
                        retryCount = int.Parse(Configuration["EventBusRetryCount"]);
                    }

                    return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
                });
            }

            RegisterEventBus(services);

			//create autofac based service provider
			ContainerBuilder container = new ContainerBuilder();
            container.Populate(services);


            return new AutofacServiceProvider(container.Build());
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            app.Map("/liveness", lapp => lapp.Run(async ctx => ctx.Response.StatusCode = 200));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        }


        private void RegisterEventBus(IServiceCollection services)
        {
			string subscriptionClientName = Configuration["SubscriptionClientName"];

            if (Configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
                {
					IServiceBusPersisterConnection serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
					ILifetimeScope iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
					ILogger<EventBusServiceBus> logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
					IEventBusSubscriptionsManager eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    return new EventBusServiceBus(serviceBusPersisterConnection, logger,
                        eventBusSubcriptionsManager, subscriptionClientName, iLifetimeScope);
                });
            }
            else
            {
                services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
                {
					IRabbitMQPersistentConnection rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
					ILifetimeScope iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
					ILogger<EventBusRabbitMQ> logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
					IEventBusSubscriptionsManager eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

					int retryCount = 5;
                    if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
                    {
                        retryCount = int.Parse(Configuration["EventBusRetryCount"]);
                    }

                    return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
                });
            }

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
        }
    }
}
