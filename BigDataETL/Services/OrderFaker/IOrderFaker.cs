using BigDataETL.Data.Models;

namespace BigDataETL.Services.DataFaker;

public interface IOrderFaker
{
    List<Order> GenerateRandomOrders(int amount);
}