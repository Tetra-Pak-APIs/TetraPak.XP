using System.Text.Json;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Configuration
{
    public class ConfigurationSectionFactory : IDynamicEntityFactory
    {
        DynamicEntity IDynamicEntityFactory.DeserializeEntity(string? key, ref Utf8JsonReader reader)
        {
            var entity = JsonSerializer.Deserialize<DynamicEntity>(ref reader)!;
            var section = new ConfigurationSection(XpServices.Get<ILog>()) { Key = key! };
            return section.WithValuesFrom(entity);
        }
    }
}