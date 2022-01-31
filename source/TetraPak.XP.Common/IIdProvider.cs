namespace TetraPak
{
    public interface IIdProvider<out TId>
    {
        TId Id { get; }
    }
}