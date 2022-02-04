using System.Text.Json;

namespace TetraPak.XP.DynamicEntities
{
    public interface IDynamicEntityFactory
    {
        public DynamicEntity DeserializeEntity(string? key, ref Utf8JsonReader reader);
    }
}