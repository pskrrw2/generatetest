using Domain.Common.Const;
using Domain.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("MatchEvents", Schema = SchemaNames.Dbo)]
public class MatchEvent :AuditableEntity
{
    [Key]
    public int EventId { get; set; }
    public required string EventName { get; set; }
    public string? EventVenue { get; set; }
    public string? EventSession { get; set; }
    public DateTimeOffset? EventDate { get; set; }
    public string? EventThumbnail { get; set; }
    public int? EventTotalParking { get; set; }
    public int? EventTotalSROTIckets { get; set; }
	public bool IsActive { get; set; }
	public string? EventTime { get; set; }
	public string? EventType { get; set; }
	public int? EventTotalTickets { get; set; }
	public int? EventSROPerTicketPrice { get; set; }
}
