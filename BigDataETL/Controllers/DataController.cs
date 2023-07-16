using BigDataETL.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BigDataETL.Controllers;

[ApiController]
public class DataController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly EtlDbContext _etlDbContext;
    private readonly ILogger<DataController> _logger;

    public DataController(IConfiguration configuration, EtlDbContext etlDbContext, ILogger<DataController> logger)
    {
        _configuration = configuration;
        _etlDbContext = etlDbContext;
        _logger = logger;
    }

    [HttpGet("test")]
    public async Task<ActionResult<ConnectionStrings>> Test()
    {
        var x = await _etlDbContext.Orders.CountAsync();

        var connectionStrings = _configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
        _logger.LogInformation("DB Connection string: {$DbConnectionString}", connectionStrings?.DbConnectionString);
        return connectionStrings;
    }
}