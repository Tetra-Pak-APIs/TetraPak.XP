using Microsoft.Extensions.DependencyInjection;
using TetraPak.XP.OAuth2.AuthCode;
using TetraPak.XP.OAuth2.ClientCredentials;
using TetraPak.XP.OAuth2.DeviceCode;

namespace mobileClient.ViewModels
{
   public static class ViewModelHelper
   {
        public static IServiceCollection AddViewModels(this IServiceCollection collection)
        {
            collection.AddSingleton(p 
                => new AuthCodeVM(p.GetRequiredService<IAuthorizationCodeGrantService>()));
            collection.AddSingleton(p 
                => new ClientCredentialsVM(p.GetRequiredService<IClientCredentialsGrantService>()));
            collection.AddSingleton(p 
                => new DeviceCodeVM(p.GetRequiredService<IDeviceCodeGrantService>()));
            return collection;
        }
    }
}