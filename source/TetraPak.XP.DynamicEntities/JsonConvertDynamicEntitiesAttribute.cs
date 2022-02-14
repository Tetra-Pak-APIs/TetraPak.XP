using System;
using System.Text.Json;

namespace TetraPak.XP.DynamicEntities
{
    [AttributeUsage(AttributeTargets.Class)]
    public class JsonConvertDynamicEntitiesAttribute : Attribute
    {
        Type? _all;
        Type? _factoryType;

        public Type? FactoryType
        {
            get => _factoryType;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value), $"Factory cannot be assigned a null value");
                
                if (_all is { })
                    throw new InvalidOperationException($"You cannot use both '{nameof(All)}' and '{nameof(FactoryType)}' for {this}");
                
                if (!value.IsImplementingInterface<IDynamicEntityFactory>())
                    throw new ArgumentException($"Entity factory must implement {typeof(IDynamicEntityFactory)}");

                var ctor = value.GetConstructor(Type.EmptyTypes);
                if (ctor is null)
                    throw new ArgumentException($"Factory must support a parameter-less constructor");

                _factoryType = value;
                Factory = (IDynamicEntityFactory?) Activator.CreateInstance(_factoryType, null);
            }
        }

        internal IDynamicEntityFactory? Factory { get; private set; }

        public Type? All
        {
            get => _all;
            set
            {
                if (Factory is { })
                    throw new InvalidOperationException($"You cannot use both '{nameof(All)}' and '{nameof(FactoryType)}' for {this}");

                _all = value;
            }
        }

        internal T Construct<T>(string? key, ref Utf8JsonReader reader) where T : DynamicEntity
        {
            if (All is { })
                return (T) JsonSerializer.Deserialize(ref reader, All);
                    
            return (T) Factory!.DeserializeEntity(key, ref reader);
        }
    }
}