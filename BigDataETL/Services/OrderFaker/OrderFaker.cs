using BigDataETL.Data.Models;
using Bogus;

namespace BigDataETL.Services.DataFaker;

internal class OrderFaker : IOrderFaker
{
    private readonly Faker<Order>? _orderFaker;

    public OrderFaker()
    {
        var lineItemEventFaker = new Faker<LineItemEvent>("en")
                .RuleFor(e => e.Id, faker => faker.Database.Random.Guid())
                .RuleFor(e => e.UtcCreatedAt, _ => DateTime.UtcNow)
                .RuleFor(e => e.EventType, faker => faker.PickRandom<LineItemEventType>())
                .RuleFor(e => e.PreviousStatus, faker => faker.PickRandom<LineItemStatus>())
                .RuleFor(e => e.NewStatus, faker => faker.PickRandom<LineItemStatus>())
            ;
        var lineItemFaker = new Faker<LineItem>()
                .RuleFor(item => item.Id, faker => faker.Database.Random.Guid())
                .RuleFor(e => e.UtcCreatedAt, _ => DateTime.UtcNow)
                .RuleFor(item => item.Status, faker => faker.PickRandom<LineItemStatus>())
                .RuleFor(item => item.Events, faker => lineItemEventFaker.Generate(10))
                .FinishWith((faker, item) => item.Events.ForEach(e =>
                {
                    e.LineItemId = item.Id;
                    e.LineItem = item;
                }))
            ;

        var orderEventFaker = new Faker<OrderEvent>()
                .RuleFor(e => e.Id, faker => faker.Database.Random.Guid())
                .RuleFor(e => e.UtcCreatedAt, _ => DateTime.UtcNow)
                .RuleFor(e => e.EventType, faker => faker.PickRandom<OrderEventType>())
                .RuleFor(e => e.PreviousStatus, faker => faker.PickRandom<OrderStatus>())
                .RuleFor(e => e.NewOrderStatus, faker => faker.PickRandom<OrderStatus>())
            ;
        _orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, faker => faker.Database.Random.Guid())
            .RuleFor(e => e.UtcCreatedAt, _ => DateTime.UtcNow)
            .RuleFor(o => o.Status, faker => faker.PickRandom<OrderStatus>())
            .RuleFor(o => o.ExternalId, faker => faker.Random.Hexadecimal(12))
            .RuleFor(o => o.Events, faker => orderEventFaker.Generate(10))
            .RuleFor(o => o.LineItems, faker => lineItemFaker.Generate(10))
            .FinishWith((faker, order) =>
            {
                order.Events.ForEach(e => e.OrderId = order.Id);
                order.Events.ForEach(e => e.Order = order);
                order.LineItems.ForEach(li => li.OrderId = order.Id);
                order.LineItems.ForEach(li => li.Order = order);
            });
    }

    public List<Order> GenerateRandomOrders(int amount)
    {
        return _orderFaker.Generate(amount);
    }
}