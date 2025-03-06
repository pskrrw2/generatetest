using Domain.Common.Const;
using Domain.Common.Enums;
using Domain.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("AttendeeMaster", Schema = SchemaNames.Dbo)]
public class Attendee : AuditableEntity
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? EmailId { get; set; }
    public string? MobileNumber { get; set; }
    public int? TicketsAssigned { get; set; }
    public string? NoteToAttendee { get; set; }
    public int? RequestId { get; set; }
    public string? SeatType { get; set; }
    public string? UserId { get;set; }
    public int? ConferenceId { get; set; }
}
