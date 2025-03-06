using System.ComponentModel.DataAnnotations;

namespace Domain.Common.Enums;
public enum StatusType
{
    [Display(Name = "Approved")]
    Approved = 1,

    [Display(Name = "Pending")]
    Pending = 2,

    [Display(Name = "Rejected")]
    Rejected = 3
}