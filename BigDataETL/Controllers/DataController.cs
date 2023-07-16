using BigDataETL.Data;
using BigDataETL.Data.Models;
using BigDataETL.Services.DataFaker;
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

    [HttpGet("test")]
    public async Task<ActionResult<object>> Test()
    {
        var x = await _etlDbContext.Orders.CountAsync();
        _etlDbContext.Orders.Add(new Order()
        {
            Id = Guid.NewGuid(),
            ExternalId = Guid.NewGuid().ToString(),
            LineItems = new List<LineItem>(),
            Events = new List<OrderEvent>(),
            Status = OrderStatus.OS1
        });
        await _etlDbContext.SaveChangesAsync();

        var orders = _orderFaker.GenerateRandomOrders(2);
        return orders;
    }
}