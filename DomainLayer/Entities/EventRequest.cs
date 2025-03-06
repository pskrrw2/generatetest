using Domain.Common.Const;
using Domain.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("EventRequests", Schema = SchemaNames.Dbo)]
public class EventRequest: AuditableEntity
{
    [Key]
    public int RequestId { get; set; }
    public int AppliedParkingPasses { get; set; }
    public string? ExecutiveNotes { get; set; }
    public string? AdminNotes { get; set; }
    public int AppliedTickets { get; set; }
    public int ApprovedTickets { get; set; }
    public int ApprovedParkingPasses { get; set; }
    public bool? SROTickets { get; set; }
    public bool? CateringAndDrinks { get; set; }
    public int EventId { get; set; }

    [ForeignKey("EventId")]
    public MatchEvent? Event { get; set; }
    public string? Status { get; set; }
    public int? AppliedSROTickets { get; set; }
    public int? ApprovedSROTickets { get; set; }
    public string? PackageSelected { get; set; }
    public string? AddonsSelected { get; set; }
    public DateTimeOffset? AddOnDate{ get; set; }
    public DateTimeOffset? PackageDate{ get; set; }
    public DateTimeOffset? ApproveRejectDate{ get; set; }
}
