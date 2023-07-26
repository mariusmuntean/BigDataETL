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

    private static async IAsyncEnumerable<Stream> GetOrderStreams(IAsyncEnumerable<Order> orders, int blockSize = 4 * 1_000_000)
    {
        var tempBuffer = new byte[blockSize];
        var currentStream = new MemoryStream();
        var orderWriter = new OrderWriter();

        // Start JSON content and add an 'orders' array
        await orderWriter.Start(currentStream);

        // Add each order and return stream chunks if necessary.
        await foreach (var order in orders)
        {
            // Add current order to the stream
            await orderWriter.WriteOrder(currentStream, order);

            // While there is more than a blockSize of data in the stream, return chunks from it.
            currentStream.Seek(0, SeekOrigin.Begin);
            while (currentStream.Length - currentStream.Position > blockSize)
            {
                // Return a new stream chunk with blockSize bytes from the current stream
                var readByteCount = await currentStream.ReadAsync(tempBuffer, 0, blockSize);
                var streamChunk = new MemoryStream(tempBuffer, 0, readByteCount);
                yield return streamChunk;
            }

            // Copy the remaining bytes into a new stream and set that one as the current stream.
            // Otherwise the stream will hold all the serialized orders simultaneously.
            var newStream = new MemoryStream();
            if (currentStream.Position < currentStream.Length)
            {
                await currentStream.CopyToAsync(newStream);
            }

            currentStream = newStream;
        }

        // Close the JSON array property and object
        await orderWriter.End(currentStream);

        // Return the current stream in chunks, as it might have exceeded the blockSize after the last write.
        currentStream.Seek(0, SeekOrigin.Begin);
        while (currentStream.Position < currentStream.Length)
        {
            var readByteCount = await currentStream.ReadAsync(tempBuffer, 0, blockSize);
            var streamChunk = new MemoryStream(tempBuffer, 0, readByteCount);
            yield return streamChunk;
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