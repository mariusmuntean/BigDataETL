using Azure.Storage.Blobs;
using BigDataETL.Data;
using BigDataETL.Services.DataFaker;

namespace BigDataETL.Services;

public static class ServicesRegistrator
{
    public static IServiceCollection RegisterCustomServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IOrderFaker, OrderFaker>();
        serviceCollection.AddScoped<IOrderProducerService, OrderProducerService>();
        serviceCollection.AddSingleton(provider =>
        {
            var connectionStrings = provider.GetRequiredService<IConfiguration>().GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>();
            var sasUri = new Uri(connectionStrings.BlobContainerSasUri);
            return new BlobContainerClient(sasUri);
        });

        return serviceCollection;
    }
}