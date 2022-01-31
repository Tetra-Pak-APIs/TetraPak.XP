namespace TetraPak.XP
{
    public interface IIdProvider<out TId>
    {
        TId Id { get; }
    }
}