using BlogGateway.Presentation.Rest.Auth.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using OpaqueScheme = BlogGateway.Presentation.Rest.Auth.Authentication.AuthenticationScheme;
using OpaqueTokenHandler = BlogGateway.Presentation.Rest.Auth.Authentication.OpaqueTokenAuthenticationHandler;

namespace BlogGateway.Presentation.Rest.Auth.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRestAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGrpcClient<AuthService.Presentation.Grpc.Protos.Auth.AuthClient>(o =>
        {
            GrpcClientOptions options = configuration
                .GetRequiredSection("GrpcClients:AuthService")
                .Get<GrpcClientOptions>()!;

            o.Address = new Uri(options.Address);
        });

        services
            .AddAuthentication(OpaqueScheme.Name)
            .AddScheme<AuthenticationSchemeOptions, OpaqueTokenHandler>(OpaqueScheme.Name, _ => { });

        services.AddAuthorization();

        services.AddControllers();
        services.AddOpenApi();
        return services;
    }
}
