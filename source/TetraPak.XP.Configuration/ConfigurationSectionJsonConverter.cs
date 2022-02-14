// using System.Text.Json;
// using TetraPak.XP.DynamicEntities;
//
// namespace TetraPak.XP.Configuration
// {
//     // nisse Consider removing (I don't think it's needed)
//     public class ConfigurationSectionJsonConverter : DynamicEntityJsonConverter<ConfigurationSection>
//     {
//         protected override DynamicEntity OnConstructEntity(string? key, ref Utf8JsonReader reader, JsonSerializerOptions options)
//         {
//             var entity = base.OnConstructEntity(key, ref reader, options);
//             if (entity is ConfigurationSection section && section.Key.IsUnassigned() && key.IsAssigned())
//             {
//                 section.Key = key!;
//             }
//
//             return entity;
//         }
//     }
// }