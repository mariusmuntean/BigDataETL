namespace BigDataETL.Data.Models;

public class Order: BaseEntity
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderEvent> Events { get; set; }
    public List<LineItem> LineItems { get; set; }

    public string ExternalId { get; set; }
}