using Domain.Common.Enums;

namespace Application.Dto;

public class AttendeeDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? EmailId { get; set; }
    public string? MobileNumber { get; set; }
    public int? RequestId { get; set; }
    public int? TicketsAssigned { get; set; }
    public string? NoteToAttendee { get; set; }
    public string? UserId { get; set; }
    public SeatType? AttendeeSeatType { get; set; }
    public int? ConferenceId { get; set; }

}
