using BigDataETL.Data.Models;

namespace BigDataETL.Services.OrderAccess;

/// <summary>
/// Access stored <see cref="Order"/>s
/// </summary>
public interface IOrderAccessService
{
    /// <summary>
    /// Get a stream of <see cref="Order"/> instances, based on the provided filter.
    /// </summary>
    /// <param name="ordersFilter"></param>
    /// <returns></returns>
    IAsyncEnumerable<Order> GetOrders(OrdersFilter ordersFilter);

    /// <summary>
    /// Filter the orders based on multiple criteria.
    /// </summary>
    /// <param name="From"></param>
    /// <param name="To"></param>
    /// <param name="Amount"></param>
    public record OrdersFilter(DateTime From, DateTime To, int? Amount);
}