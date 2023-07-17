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
    private readonly ILogger<DataController> _logger;

    public DataController(EtlDbContext etlDbContext, IOrderFaker orderFaker, IOrderProducerService orderProducerService, ILogger<DataController> logger)
    {
        _etlDbContext = etlDbContext;
        _orderFaker = orderFaker;
        _orderProducerService = orderProducerService;
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
}