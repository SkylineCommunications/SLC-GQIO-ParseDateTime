using System.Globalization;
using System;
using System.Linq;

internal static class Localization
{
    public static string[] GetCultureOptions()
    {
        return CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Select(culture => culture.Name)
            .ToArray();
    }

    public static string[] GetTimeZoneOptions()
    {
        return TimeZoneInfo.GetSystemTimeZones()
            .Select(timeZone => timeZone.Id)
            .ToArray();
    }

    public static CultureInfo GetCulture(string cultureValue)
    {
        if (string.IsNullOrEmpty(cultureValue))
            return CultureInfo.InvariantCulture;

        try
        {
            return CultureInfo.GetCultureInfo(cultureValue);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }

    public static TimeZoneInfo GetTimeZone(string timeZoneValue)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneValue);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
