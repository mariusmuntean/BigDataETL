using BigDataETL.Data;
using BigDataETL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BigDataETL.Services.OrderAccess;

internal class OrderAccessService : IOrderAccessService
{
    private readonly EtlDbContext _etlDbContext;

    public OrderAccessService(EtlDbContext etlDbContext)
    {
        _etlDbContext = etlDbContext;
    }

    public IAsyncEnumerable<Order> GetOrders(IOrderAccessService.OrdersFilter ordersFilter)
    {
        var ordersQueryable = _etlDbContext.Orders.AsNoTracking()
            .Include(order => order.Events)
            .Include(order => order.LineItems)
            .ThenInclude(item => item.Events)
            .Where(order => order.UtcCreatedAt >= ordersFilter.From && order.UtcCreatedAt <= ordersFilter.To);
        
        ordersQueryable = ordersFilter.Amount.HasValue ? ordersQueryable.Take(ordersFilter.Amount.Value) : ordersQueryable;

        return ordersQueryable.AsAsyncEnumerable();
    }
}