using Azure.Messaging.ServiceBus;
using FCG.Shared.EventService.Consumer;
using FCG.Shared.EventService.Consumer.ServiceBus;
using FCG.Shared.EventService.Publisher;
using FCG.Shared.EventService.Publisher.ServiceBus;
using FCG_Payments.Application.Payments.Handlers;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;
using FCG_Payments.Infrastructure.Payments.Repositories;
using FCG_Payments.Infrastructure.Shared.Context;
using Microsoft.EntityFrameworkCore;

namespace FCG_Payments.Consumer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddConsumerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PaymentDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            var connectionStringServiceBus = configuration["ServiceBus:ConnectionString"];
            services.AddSingleton(new ServiceBusClient(connectionStringServiceBus));

            services.AddScoped<IRepository<Payment>, PaymentRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            services.AddScoped<IEventServicePublisher>(sp =>
            {
                var client = sp.GetRequiredService<ServiceBusClient>();
                return new ServiceBusEventPublisher(client);
            });

            services.AddScoped<IMessageHandler, LibraryOrderHandler>();
            services.AddScoped<IMessageHandler, PaymentAprovedHandler>();
            services.AddScoped<IMessageHandler, PaymentFailedHandler>();

            services.AddScoped<ServiceBusMessageDispatcher>();
            services.AddScoped<IQueueConsumer, QueueConsumer>();

            return services;
        }
    }
}
