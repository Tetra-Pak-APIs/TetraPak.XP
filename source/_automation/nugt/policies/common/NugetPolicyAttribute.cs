using System;

namespace nugt.policies
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NugetPolicyAttribute : Attribute
    {
        public string Name { get; }

        public NugetPolicyAttribute(string name)
        {
            Name = name;
        }
    }
}