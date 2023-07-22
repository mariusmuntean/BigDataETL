using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using BigDataETL.Data.Models;

namespace BigDataETL.Services.OrderToBlobUploader;

/// <inheritdoc cref="IOrderToBlobUploader"/>
internal class OrderToBlobUploader : IOrderToBlobUploader
{
    private readonly BlobContainerClient _blobContainerClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public OrderToBlobUploader(BlobContainerClient blobContainerClient)
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

    public async ValueTask<BlockBlobClient> UploadOrdersEfficiently(IAsyncEnumerable<Order> orders, int blockSize = 4 * 1_000_000)
    {
        var blobName = Guid.NewGuid() + ".json";
        var blobClient = _blobContainerClient.GetBlockBlobClient(blobName);

        var blockIds = new List<string>();
        await foreach (var blockStream in GetOrderStreams(orders, blockSize))
        {
            var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

            await blobClient.StageBlockAsync(blockId, blockStream);
            blockIds.Add(blockId);
        }

        await blobClient.CommitBlockListAsync(blockIds);

        return blobClient;
    }

    private async IAsyncEnumerable<Stream> GetOrderStreams(IAsyncEnumerable<Order> orders, int blockSize = 4 * 1_000_000)
    {
        // ToDo: maybe use Span<byte> instead of copying so much memory around

        var currentStream = new MemoryStream();
        await foreach (var order in orders)
        {
            await JsonSerializer.SerializeAsync(currentStream, order, _jsonSerializerOptions); // ToDo: switch to Utf8JsonWriter
            while (currentStream.Length > blockSize)
            {
                currentStream.Seek(0, SeekOrigin.Begin);

                // Return a new stream with blockSize bytes from the current stream
                var destinationBuffer = new byte[blockSize];
                var readByteCount = await currentStream.ReadAsync(destinationBuffer, 0, blockSize);
                yield return new MemoryStream(destinationBuffer, 0, readByteCount);

                // Copy the remaining bytes into a new stream and set that one as the current stream
                var newStream = new MemoryStream();
                await currentStream.CopyToAsync(newStream);
                currentStream = newStream;
            }
        }

        // Return the last remaining bytes from the stream, if any
        if (currentStream.Length > 0)
        {
            currentStream.Seek(0, SeekOrigin.Begin);
            yield return currentStream;
        }
    }
}