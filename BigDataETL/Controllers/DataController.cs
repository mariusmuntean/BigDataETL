using BigDataETL.Data;
using BigDataETL.Data.Models;
using BigDataETL.Services.DataFaker;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BigDataETL.Controllers;

[ApiController]
public class DataController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly EtlDbContext _etlDbContext;
    private readonly IOrderFaker _orderFaker;
    private readonly ILogger<DataController> _logger;

    public DataController(IConfiguration configuration, EtlDbContext etlDbContext, IOrderFaker orderFaker, ILogger<DataController> logger)
    {
        _configuration = configuration;
        _etlDbContext = etlDbContext;
        _orderFaker = orderFaker;
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
    
    [HttpGet("testget/{amount:int}")]
    public object GetSomeOrderData([FromRoute] int amount)
    {
        var ordersQueryable = _etlDbContext.Orders.AsNoTracking()
            .Include(order => order.Events)
            .Include(order => order.LineItems)
            .ThenInclude(item => item.Events)
            .Take(amount);

        return EnumerateIds(ordersQueryable);
    }

    private static IEnumerable<Guid> EnumerateIds(IQueryable<Order> ordersQueryable)
    {
        foreach (var order in ordersQueryable)
        {
            yield return order.Id;
        }
    }
}