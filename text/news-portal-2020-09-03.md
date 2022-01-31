# API authorization now supports OIDC

One of the most requested capabilities for the TAX package is the ability for mobile clients to also access information about the authorized user, such as his/her name, email and so on. This feature was implemented in Tetra Pak's Login API a few sprints back, by adding support for OIDC. Typically, when asking for access tokens with OAuth2, the client simply adds additional "scopes" to the request



The problem was that the TAX package, which aims to automate s much as possible for the developer, did not provide a code API to allow consumtion 

Tetra Pak Authorization for Xamarin (or just "TAX" for short) is a code component that greatly simplifies authorizing native mobile apps. The package enables any Xamarin project to start consuming [Tetra Pak APIs][tetra-pak-developer-portal] in less than twenty minutes.

Relying on ready-made components is a great way for any mobile app project to get complex cross cutting concerns (C3) such as authorization out of the way and instead focus on adding business value while staying on top of changing security policies.

Last week the Bold Eagles team released version 1.1.1 of the **TAX** NuGet package. This version supports requesting user's identity (through [Open ID Connect][OIDC]) . This is a common requirement in many client projects and for consuming several APIs as well.

While releasing this new version the team decided it is time to go open source, making it available to the world, and published the code at GitHub - the world's largest code repository. The compiled code library itself is now also available at [nuget.org][NuGet], the primary global source for .NET code packages. This means any .NET based client project anywhere in the world can now be up and running in minutes, consuming Tetra Pak data!

To learn more about the TAX component please check out the details from the [README at GitHub][readme]. When your team is ready to use it just direct your favorite IDE to [nuget.org][NuGet] and search for "TetraPak.Auth.Xamarin".

Happy coding!

-The Bold Eagles team

[NuGet]: https://nuget.org
[OIDC]: https://openid.net/connect/
[readme]: https://github.com/Tetra-Pak-APIs/TetraPak.Auth.Xamarin
[tetra-pak-developer-portal]: https://developer.TetraPak.com