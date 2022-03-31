using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace TetraPak.XP.DependencyInjection;

public static class ServiceCollectionHelper
{
    public static bool IsAdded<T>(this IServiceCollection collection)
    {
        return collection.Any(i => i.ServiceType == typeof(T));
    }
}