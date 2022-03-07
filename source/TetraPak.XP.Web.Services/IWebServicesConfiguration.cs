using Microsoft.Extensions.Configuration;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.Web.Services
{
    // todo move to a more "web" oriented lib

    public interface IWebServicesConfiguration : IConfigurationSection
    {
        IWebServiceConfiguration? GetConfiguration(string serviceName);
    }
}