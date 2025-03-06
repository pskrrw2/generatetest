using Microsoft.AspNetCore.Identity;

namespace Domain.Identity;

public class ApplicationUserRole : IdentityUserRole<string>, IEquatable<ApplicationUserRole>
{
    public ApplicationUser? User { get; set; }
    public ApplicationRole? Role { get; set; }

    public bool Equals(ApplicationUserRole? other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return User.Equals(other.User) && Role.Equals(other.Role);
    }

    public override string? ToString()
    {
        return Role.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return obj.GetType() == GetType() && Equals((ApplicationUserRole)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(User, Role);
    }
}