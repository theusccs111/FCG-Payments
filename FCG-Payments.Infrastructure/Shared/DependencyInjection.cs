using Azure.Messaging.ServiceBus;
using FCG.Shared.EventLog.Publisher;
using FCG.Shared.EventLog.Publisher.MongoDB;
using FCG.Shared.EventService.Publisher;
using FCG.Shared.EventService.Publisher.ServiceBus;
using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Domain.Payments.Entities;
using FCG_Payments.Infrastructure.Payments.Repositories;
using FCG_Payments.Infrastructure.Payments.Strategy;
using FCG_Payments.Infrastructure.Shared.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace FCG_Payments.Infrastructure.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PaymentDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            var connectionString = configuration["ServiceBus:ConnectionString"];
            var connectionStringMongodb = configuration["Mongodb:ConnectionString"];

            services.AddSingleton(new ServiceBusClient(connectionString));
            services.AddSingleton(new MongoClient(connectionStringMongodb));

            services.AddScoped<IEventServicePublisher>(sp =>
            {
                var client = sp.GetRequiredService<ServiceBusClient>();
                return new ServiceBusEventPublisher(client);
            });

            services.AddScoped<IEventLogPublisher>(sp =>
            {
                var client = sp.GetRequiredService<MongoClient>();
                var databaseName = configuration["Mongodb:DatabaseName"];
                var collectionName = configuration["Mongodb:CollectionName"];

                return new MongoDBEventLogPublisher(client, databaseName!, collectionName!);
            });

            services.AddScoped<IRepository<Payment>, PaymentRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentResolver, PaymentFactory>();

            return services;
        }
    }
}
