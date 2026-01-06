using FCG_Payments.Application.Payments.Services;
using FCG_Payments.Application.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FCG_Payments.Application.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IPaymentService, PaymentService>();           

            return services;
        }
    }
}