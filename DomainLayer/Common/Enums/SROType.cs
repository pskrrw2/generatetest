using System.ComponentModel.DataAnnotations;

namespace Domain.Common.Enums;

public enum SROType
{
    [Display(Name = "I don't want SRO passes")]
    No = 0,

    [Display(Name = "I want SRO passes")]
    Yes = 1
}
