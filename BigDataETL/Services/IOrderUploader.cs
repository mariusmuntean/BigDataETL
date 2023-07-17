using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using BigDataETL.Data.Models;

namespace BigDataETL.Services;

public interface IOrderUploader
{
    ValueTask<BlockBlobClient> UploadOrdersOnePerBlock(IAsyncEnumerable<Order> orders);
}

public class OrderUploader : IOrderUploader
{
    private readonly BlobContainerClient _blobContainerClient;
    private JsonSerializerOptions _jsonSerializerOptions;

    public OrderUploader(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };
    }

    public async ValueTask<BlockBlobClient> UploadOrdersOnePerBlock(IAsyncEnumerable<Order> orders)
    {
        var blobName = Guid.NewGuid() + ".json";
        var blobClient = _blobContainerClient.GetBlockBlobClient(blobName);

        var blockIds = new List<string>();
        await foreach (var order in orders)
        {
            var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, order, _jsonSerializerOptions);
            ms.Seek(0, SeekOrigin.Begin);

            await blobClient.StageBlockAsync(blockId, ms);
            blockIds.Add(blockId);
        }

        await blobClient.CommitBlockListAsync(blockIds);

        return blobClient;
    }
}