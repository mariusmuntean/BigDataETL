using BigDataETL.Data;
using BigDataETL.Data.Models;
using BigDataETL.Services;
using BigDataETL.Services.DataFaker;
using BigDataETL.Services.OrderAccess;
using BigDataETL.Services.OrderToBlobUploader;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BigDataETL.Controllers;

[ApiController]
public class DataController : ControllerBase
{
    private readonly EtlDbContext _etlDbContext;
    private readonly IOrderFaker _orderFaker;
    private readonly IOrderToBlobUploader _orderToBlobUploader;
    private readonly IOrderAccessService _orderAccessService;
    private readonly ILogger<DataController> _logger;

    public DataController(EtlDbContext etlDbContext, IOrderFaker orderFaker, IOrderToBlobUploader orderToBlobUploader, IOrderAccessService orderAccessService, ILogger<DataController> logger)
    {
        _etlDbContext = etlDbContext;
        _orderFaker = orderFaker;
        _orderToBlobUploader = orderToBlobUploader;
        _orderAccessService = orderAccessService;
        _logger = logger;
    }

    [HttpPost("order/{amount:int}")]
    public async Task<ActionResult<object>> AddRandomOrders([FromRoute] int amount)
    {
        var orders = _orderFaker.GenerateRandomOrders(amount);
        await _etlDbContext.BulkInsertAsync(orders, config => config.IncludeGraph = true);

        return orders.Count;
    }
    
    [HttpGet("order/count")]
    public async Task<ActionResult<object>> GetOrderCount()
    {
        return await _etlDbContext.Orders.CountAsync();
    }

    [HttpGet("order")]
    public IAsyncEnumerable<Order> GetSomeOrderData([FromQuery] IOrderAccessService.OrdersFilter ordersFilter)
    {
        return _orderAccessService.GetOrders(ordersFilter);
    }

    [HttpPost("etl")]
    public async Task<object> DumpOrderData([FromQuery] IOrderAccessService.OrdersFilter ordersFilter)
    {
        var orders = _orderAccessService.GetOrders(ordersFilter);
        var blockBlobClient = await _orderToBlobUploader.UploadOrdersOnePerBlock(orders);

        return blockBlobClient;
    }
    
    [HttpPost("etlefficient")]
    public async Task<object> DumpOrderDataEfficient([FromQuery] IOrderAccessService.OrdersFilter ordersFilter)
    {
        var orders = _orderAccessService.GetOrders(ordersFilter);
        var blockBlobClient = await _orderToBlobUploader.UploadOrdersEfficiently(orders, 5_000_000);

        return blockBlobClient;
    }
}