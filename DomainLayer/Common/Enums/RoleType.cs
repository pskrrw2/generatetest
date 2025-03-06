using System.ComponentModel.DataAnnotations;

namespace Domain.Common.Enum;

public enum RoleType
{
    [Display(Name = "Admin")]
    Admin = 1,

    [Display(Name = "Executive")]
    Executive = 2
}
