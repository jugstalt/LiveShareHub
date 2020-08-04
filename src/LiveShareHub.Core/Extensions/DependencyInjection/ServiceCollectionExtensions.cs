using LiveShareHub.Core.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace LiveShareHub.Core.Extensions.DependencyInjection
{
    static public class ServiceCollectionExtensions
    {
        static public IServiceCollection AddGroupProviderService<T,TOptions>(this IServiceCollection services, Action<TOptions> configureOptions)
            where T : class, IGroupIdProvider
            where TOptions: class
        {
            services.Configure(configureOptions);
            return services.AddTransient<IGroupIdProvider, T>();
        }
    }
}
