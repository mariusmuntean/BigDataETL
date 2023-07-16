namespace BigDataETL.Data.Models;

public class LineItemEvent : BaseEntity
{
    public Guid Id { get; set; }
    public LineItemEventType EventType { get; set; }
    public LineItemStatus? PreviousStatus { get; set; }
    public LineItemStatus? NewStatus { get; set; }

    public LineItem LineItem { get; set; }
    public Guid LineItemId { get; set; }
}