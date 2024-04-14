using HomeworkApp.Utils.Providers;
using HomeworkApp.Utils.Providers.Interfaces;

namespace HomeworkApp.Utils.Extensions;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUtils(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDateTimeOffsetProvider, DateTimeOffsetProvider>();

        return serviceCollection;
    }
}