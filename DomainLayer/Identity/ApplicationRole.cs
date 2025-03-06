using Microsoft.AspNetCore.Identity;

namespace Domain.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    //public List<ApplicationUserRole> UserRoles { get; set; } = new();

    public ApplicationRole(string name, string? description = null)
        : base(name)
    {
        Description = description;
        NormalizedName = name.ToUpperInvariant();
    }
}