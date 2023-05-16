using ApiApplication.API;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ApiApplication.Extensions
{
    //resolver delegate
    public delegate IApiClient ReminderServiceResolver(string identifier);
    public static class DependencyRegistrationBoostrapper
    {
        public static void RegisterProtocolDependencies(this IServiceCollection services)
        {
            services.AddSingleton<ApiClientHttp>();
            services.AddSingleton<ApiClientGrpc>();

            services.AddSingleton<ReminderServiceResolver>(serviceProvider => token =>
            {
                return token switch
                {
                    "Http" => serviceProvider.GetService<ApiClientHttp>(),
                    "Https" => serviceProvider.GetService<ApiClientGrpc>(),
                    _ => throw new InvalidOperationException()
                };
            });
        }
    }
}
