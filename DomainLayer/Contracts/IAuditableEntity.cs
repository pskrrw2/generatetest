namespace Domain.Contracts;

public interface IAuditableEntity
{
    public string? CreatedBy { get; set; }
    public DateTimeOffset CreatedOn { get; }
    public string? LastModifiedBy { get; set; }
    public DateTimeOffset? LastModifiedOn { get; set; }
}