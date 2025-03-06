using System.ComponentModel.DataAnnotations;

namespace Domain.Common.Enums;
public enum EventType
{
    [Display(Name = "Football Games")]
    FootballGames = 1,

    [Display(Name = "Soccer Matches")]
    SoccerMatches = 2,

    [Display(Name = "Concerts")]
    Concerts = 3,

    [Display(Name = "Other Entertainment")]
    OtherEntertainment = 4

}
