using BigDataETL.Data;
using BigDataETL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BigDataETL.Services;

public interface IOrderProducerService
{
    IAsyncEnumerable<Order> GetOrders(OrdersFilter ordersFilter);

    public record OrdersFilter(DateTime From, DateTime To, int? Amount);
}

internal class OrderProducerService : IOrderProducerService
{
    private readonly EtlDbContext _etlDbContext;

    public OrderProducerService(EtlDbContext etlDbContext)
    {
        _etlDbContext = etlDbContext;
    }

    public IAsyncEnumerable<Order> GetOrders(IOrderProducerService.OrdersFilter ordersFilter)
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