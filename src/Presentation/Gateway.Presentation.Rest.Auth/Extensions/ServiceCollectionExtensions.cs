using Gateway.Presentation.Rest.Auth.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Gateway.Presentation.Rest.Auth.Extensions;

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

        services.AddControllers();
        services.AddOpenApi();
        return services;
    }
}
