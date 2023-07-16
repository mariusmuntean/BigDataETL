namespace BigDataETL.Data.Models;

public class Order
{
    public Guid Id { get; set; }
    public DateTime UtcCreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderEvent> Events { get; set; }
    public List<LineItem> Type { get; set; }

    public string ExternalId { get; set; }
}