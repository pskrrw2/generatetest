namespace Infrastructure.Features;
public static class ISTDateTime
{
	public static DateTime GetDateTime() {
		var utcTime = DateTimeOffset.UtcNow;
		TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
		return TimeZoneInfo.ConvertTimeFromUtc(utcTime.DateTime, istTimeZone);
	}
}
