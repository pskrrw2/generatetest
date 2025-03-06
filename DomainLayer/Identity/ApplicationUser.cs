using Domain.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Domain.Identity;

public class ApplicationUser : IdentityUser, IAuditableEntity
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? OtpNumber { get; set; }
    public DateTimeOffset? OtpExpire { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public string? LastModifiedBy { get; set; }
    public DateTimeOffset? LastModifiedOn { get; set; }
}