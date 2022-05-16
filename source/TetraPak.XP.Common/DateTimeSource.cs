namespace TetraPak.XP
{
    public static class DateTimeSource
    {
        public static IDateTimeSource Current { get; set; } = new XpDateTime();
    }
}