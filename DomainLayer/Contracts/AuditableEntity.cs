using System.ComponentModel.DataAnnotations;

namespace Domain.Contracts;

public abstract class AuditableEntity : IAuditableEntity
{

    [StringLength(256)]
    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    [StringLength(256)]
    public string? LastModifiedBy { get; set; }

    public DateTimeOffset? LastModifiedOn { get; set; }
}