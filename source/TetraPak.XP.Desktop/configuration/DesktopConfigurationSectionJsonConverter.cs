using System.Text.Json;
using TetraPak.XP.DynamicEntities;

namespace TetraPak.XP.Desktop
{
    public class DesktopConfigurationSectionJsonConverter : DynamicEntityJsonConverter<DesktopConfigurationSection>
    {
        protected override DynamicEntity OnDeserializeArbitraryObject(string key, ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var entity = base.OnDeserializeArbitraryObject(key, ref reader, options);
            if (entity is DesktopConfigurationSection section)
            {
                section.Key = key;
            }

            return entity;
        }
    }
}