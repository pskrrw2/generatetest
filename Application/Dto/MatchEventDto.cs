using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Dto;

public class MatchEventDto
{
    public int EventId { get; set; }
    public required string EventName { get; set; }
    public string? EventVenue { get; set; }
    public DateTimeOffset? EventDate { get; set; }
    public string? EventThumbnail { get; set; }
    public required bool IsActive { get; set; }
    public IFormFile? File { get; set; }
    public string? EventSession { get; set; }
    public string? EventTime { get; set; }
    public string? EventType { get; set; }
    public int? EventTotalTickets { get; set; }
    public int? EventTotalParking { get; set; }
    public int? EventTotalSROTIckets { get; set; }
    public int? EventSROPerTicketPrice { get; set; }
}
