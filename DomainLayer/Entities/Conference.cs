using Domain.Common.Const;
using Domain.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ConferenceMaster", Schema = SchemaNames.Dbo)]
public class Conference : AuditableEntity
{
    [Key]
    public int ConferenceId { get; set; }
    public required string SuiteType { get; set; }
    public required DateTimeOffset ConferenceDate { get; set; }
    public string? ConferenceNotes { get; set; }
    public string? AdminConferenceNotes { get; set; }
    public string? Status { get; set; }
    public string? UserId { get; set; }
    public string? PackageSelected { get; set; }
    public string? AddonsSelected { get; set; }
    public DateTimeOffset? AddOnDate { get; set; }
    public DateTimeOffset? PackageDate { get; set; }
    public DateTimeOffset? ApproveRejectDate { get; set; }
}
