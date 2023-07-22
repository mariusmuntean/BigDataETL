using Azure.Storage.Blobs;
using BigDataETL.Data;
using BigDataETL.Services.DataFaker;
using BigDataETL.Services.OrderAccess;
using BigDataETL.Services.OrderToBlobUploader;

namespace BigDataETL.Services;

public static class ServicesRegistrator
{
    public static IServiceCollection RegisterCustomServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IOrderFaker, OrderFaker>();
        serviceCollection.AddScoped<IOrderAccessService, OrderAccessService>();
        
        serviceCollection.AddSingleton(provider =>
        {
            var connectionStrings = provider.GetRequiredService<IConfiguration>().GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>();
            var sasUri = new Uri(connectionStrings.BlobContainerSasUri);
            return new BlobContainerClient(sasUri);
        });

        serviceCollection.AddScoped<IOrderToBlobUploader, OrderToBlobUploader.OrderToBlobUploader>();

        return serviceCollection;
    }
}