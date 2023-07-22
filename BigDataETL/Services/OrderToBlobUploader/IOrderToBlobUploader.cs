using Azure.Storage.Blobs.Specialized;
using BigDataETL.Data.Models;

namespace BigDataETL.Services.OrderToBlobUploader;

/// <summary>
/// Uploads a stream of <see cref="Order"/> instances as a JSON file to the blob store.
/// <remarks>This is the essence of this repo.</remarks>
/// </summary>
public interface IOrderToBlobUploader
{
    /// <summary>
    /// Uploads the stream of <see cref="Order"/>s to a blob, each instance to a new block.
    /// <remarks>Make sure not to upload more than 50k instances</remarks> 
    /// </summary>
    /// <param name="orders"></param>
    /// <returns></returns>
    ValueTask<BlockBlobClient> UploadOrdersOnePerBlock(IAsyncEnumerable<Order> orders);

    /// <summary>
    /// Uploads a stream of <see cref="Order"/>s to a blob, with a fixed block size. New blocks are only created when previous ones are full.
    /// 
    /// Fore more info see 
    /// <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/understanding-block-blobs--append-blobs--and-page-blobs#about-block-blobs">About Block Blobs</see>
    /// <remarks>This is the most efficient way of uploading data.</remarks>
    /// </summary>
    /// <param name="orders"></param>
    /// <param name="blockSize">The size in bytes of each block. Can be any positive number up to 4000.</param>
    /// <returns></returns>
    ValueTask<BlockBlobClient> UploadOrdersEfficiently(IAsyncEnumerable<Order> orders, int blockSize = 4 * 1_000_000);
}