using System.ComponentModel.DataAnnotations;

namespace Domain.Common.Enums;

public enum SeatType
{
    [Display(Name = "High Bar")]
    HighBar = 1,

    [Display(Name = "Stadium")]
    Stadium = 2,
        
    [Display(Name = "SRO")]
    SRO = 3
}
