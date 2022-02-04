using System;

namespace TetraPak.XP.DynamicEntities
{
    [AttributeUsage(AttributeTargets.Class)]
    public class JsonConvertArbitraryObjectsAttribute : Attribute
    {
        Type? _all;
        DynamicEntityFactory? _factory;

        public DynamicEntityFactory? Factory
        {
            get => _factory;
            set
            {
                if (_all is { })
                    throw new InvalidOperationException($"You cannot use both '{All}' and '{Factory}' for {this}");

                _factory = value;
            }
        }

        public Type? All
        {
            get => _all;
            set
            {
                if (_factory is { })
                    throw new InvalidOperationException($"You cannot use both '{All}' and '{Factory}' for {this}");

                _all = value;
            }
        }
    }

    public delegate DynamicEntity DynamicEntityFactory(string key);
}