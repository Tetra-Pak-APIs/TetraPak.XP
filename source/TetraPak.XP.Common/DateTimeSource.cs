namespace TetraPak.XP
{
    public class DateTimeSource
    {
        public static IDateTimeSource Current { get; set; } = new XpDateTime();
    }
}