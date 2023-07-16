namespace BigDataETL.Data.Models;

public class LineItem : BaseEntity
{
    public Guid Id { get; set; }
    public LineItemStatus Status { get; set; }
    public List<LineItemEvent> Events { get; set; }

    public Order Order { get; set; }
    public Guid OrderId { get; set; }
}