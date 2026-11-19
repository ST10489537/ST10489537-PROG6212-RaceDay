namespace RaceDay.Web.Helpers;

// Small display helpers used by the views.
public static class Formatting
{
    // 1 -> "1st", 2 -> "2nd", 47 -> "47th", 12 -> "12th".
    public static string Ordinal(int number)
    {
        var lastTwo = number % 100;
        if (lastTwo is >= 11 and <= 13) return $"{number}th";

        return (number % 10) switch
        {
            1 => $"{number}st",
            2 => $"{number}nd",
            3 => $"{number}rd",
            _ => $"{number}th"
        };
    }

    // Shows a finish time as hh:mm:ss, e.g. 03:45:12.
    public static string FinishTime(TimeSpan time) =>
        $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
}