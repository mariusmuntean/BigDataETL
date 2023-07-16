namespace BigDataETL.Data.Models;

public class OrderEvent:BaseEntity
{
    public Guid Id { get; set; }
    public OrderEventType EventType { get; set; }
    public OrderStatus? PreviousStatus { get; set; }
    public OrderStatus? NewOrderStatus { get; set; }

    public Order Order { get; set; }
    public Guid OrderId { get; set; }
}