using BigDataETL.Data;
using Microsoft.AspNetCore.Mvc;

namespace BigDataETL.Controllers;

[ApiController]
public class DataController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataController> _logger;

    public DataController(IConfiguration configuration, ILogger<DataController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("test")]
    public ActionResult<ConnectionStrings> Test()
    {
        var connectionStrings = _configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
        _logger.LogInformation("DB Connection string: {$DbConnectionString}", connectionStrings?.DbConnectionString);
        return connectionStrings;
    }
}