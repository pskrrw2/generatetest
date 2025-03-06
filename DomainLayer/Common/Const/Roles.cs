namespace Domain.Common.Const;

public static class Roles
{
    public static readonly Role Admin = new("Admin", "Admin");
    public static readonly Role Executive = new ("Executive", "Executive");

    public static readonly IReadOnlyList<string> StaticRoles = new List<string> { Admin.Name, Executive.Name };
}

public class Role
{
    public Role(string displayText, string name)
    {
        DisplayText = displayText;
        Name = name;
    }

    public string DisplayText { get; }
    public string Name { get; }
}