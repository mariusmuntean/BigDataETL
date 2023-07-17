using BigDataETL.Services.DataFaker;

namespace BigDataETL.Services;

public static class ServicesRegistrator
{
    public static IServiceCollection RegisterCustomServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IOrderFaker, OrderFaker>();
        serviceCollection.AddScoped<IOrderProducerService, OrderProducerService>();

        return serviceCollection;
    }
}