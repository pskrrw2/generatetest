namespace Infrastructure.Features;

public static  class PSTDateTime
{

    public static DateTimeOffset? ConvertUtcToTimeZone(DateTimeOffset? utcDateTime, string timeZoneId)
    {
        if (utcDateTime == null) return null;

        TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        DateTimeOffset convertedTime = TimeZoneInfo.ConvertTime((DateTimeOffset)utcDateTime, targetTimeZone);

        return convertedTime;

    }
}
