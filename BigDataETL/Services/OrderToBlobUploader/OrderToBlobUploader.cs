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
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
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
        var tempBuffer = new byte[blockSize];
        var currentStream = new MemoryStream();
        var orderWriter = new OrderWriter();

        // Start JSON content and add an 'orders' array
        await orderWriter.Start(currentStream);

        await foreach (var order in orders)
        {
            // Add current order to the stream
            await orderWriter.WriteOrder(currentStream, order);

            // While the stream is larger than the defined block size, return blockSize chunks from it.
            while (currentStream.Length > blockSize)
            {
                currentStream.Seek(0, SeekOrigin.Begin);

                // Return a new stream with blockSize bytes from the current stream
                var readByteCount = await currentStream.ReadAsync(tempBuffer, 0, blockSize);
                yield return new MemoryStream(tempBuffer, 0, readByteCount);

                // Copy the remaining bytes into a new stream and set that one as the current stream
                var newStream = new MemoryStream();
                await currentStream.CopyToAsync(newStream);
                currentStream = newStream;
            }
        }

        // Close the JSON array property and object
        await orderWriter.End(currentStream);

        // Return the last remaining bytes from the stream, if any
        if (currentStream.Length > 0)
        {
            currentStream.Seek(0, SeekOrigin.Begin);
            yield return currentStream;
        }
    }
}

internal class OrderWriter
{
    private int _writtenOrdersCount;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public OrderWriter()
    {
        _writtenOrdersCount = 0;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
        };
    }

    public async Task Start(Stream stream)
    {
        // Start JSON object and add an 'orders' array
        var streamWriter = new StreamWriter(stream, Encoding.UTF8);
        await streamWriter.WriteLineAsync("{");
        await streamWriter.WriteLineAsync("\t\"orders\": [");
        await streamWriter.FlushAsync();
    }

    public async Task WriteOrder(Stream currentStream, Order order)
    {
        // Except for the first order, write a comma before each order.
        if (_writtenOrdersCount > 0)
        {
            var streamWriter = new StreamWriter(currentStream, Encoding.UTF8);
            await streamWriter.WriteLineAsync(",");
            await streamWriter.FlushAsync();
        }

        await JsonSerializer.SerializeAsync(currentStream, order, _jsonSerializerOptions);
        _writtenOrdersCount++;
    }

    public async Task End(Stream stream)
    {
        // Close the JSON array property and object
        var streamWriter = new StreamWriter(stream, Encoding.UTF8);
        await streamWriter.WriteLineAsync("\n\t ]");
        await streamWriter.WriteLineAsync("}");
        await streamWriter.FlushAsync();
    }
}