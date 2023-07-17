using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using BigDataETL.Data;
using BigDataETL.Data.Models;
using BigDataETL.Services;
using BigDataETL.Services.DataFaker;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BigDataETL.Controllers;

[ApiController]
public class DataController : ControllerBase
{
    private readonly EtlDbContext _etlDbContext;
    private readonly IOrderFaker _orderFaker;
    private readonly IOrderProducerService _orderProducerService;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly ILogger<DataController> _logger;

    public DataController(EtlDbContext etlDbContext, IOrderFaker orderFaker, IOrderProducerService orderProducerService, BlobContainerClient blobContainerClient, ILogger<DataController> logger)
    {
        _etlDbContext = etlDbContext;
        _orderFaker = orderFaker;
        _orderProducerService = orderProducerService;
        _blobContainerClient = blobContainerClient;
        _logger = logger;
    }

    [HttpGet("test/")]
    public async Task<ActionResult<object>> GetOrderCount()
    {
        return await _etlDbContext.Orders.CountAsync();
    }

    [HttpPost("test/{amount:int}")]
    public async Task<ActionResult<object>> AddRandomOrders([FromRoute] int amount)
    {
        var orders = _orderFaker.GenerateRandomOrders(amount);
        await _etlDbContext.BulkInsertAsync(orders, config => config.IncludeGraph = true);

        return orders.Count;
    }

    [HttpGet("order")]
    public IAsyncEnumerable<Order> GetSomeOrderData([FromQuery] IOrderProducerService.OrdersFilter ordersFilter)
    {
        return _orderProducerService.GetOrders(ordersFilter);
    }

    [HttpPost("dump")]
    public async Task<object> DumpOrderData([FromQuery] IOrderProducerService.OrdersFilter ordersFilter)
    {
        var blobName = Guid.NewGuid() + ".json";
        var blobClient = _blobContainerClient.GetBlockBlobClient(blobName);

        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };

        var blockIds = ordersFilter.Amount.HasValue ? new List<string>(ordersFilter.Amount.Value) : new List<string>();
        await foreach (var order in _orderProducerService.GetOrders(ordersFilter))
        {
            var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync(ms, order, jsonSerializerOptions);
            ms.Seek(0, SeekOrigin.Begin);
            await blobClient.StageBlockAsync(blockId, ms);
            blockIds.Add(blockId);
        }

        await blobClient.CommitBlockListAsync(blockIds);

        return blobClient;
    }
}