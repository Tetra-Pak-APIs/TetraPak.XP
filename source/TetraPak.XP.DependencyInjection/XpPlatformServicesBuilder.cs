using System;

namespace TetraPak.XP.DependencyInjection
{
    public sealed class XpPlatformServicesBuilder
    {
        readonly Type[] _types;

        public XpServicesBuilder Build() => new(_types);

        public XpPlatformServicesBuilder(Type[] types)
        {
            _types = types;
        }
    }
}