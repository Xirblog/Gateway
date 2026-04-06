using Gateway.Presentation.Rest.Users.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Gateway.Presentation.Rest.Users.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRestUsers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGrpcClient<UserService.Presentation.Grpc.Protos.UserService.UserServiceClient>(o =>
        {
            GrpcClientOptions options = configuration
                .GetRequiredSection("GrpcClients:UserService")
                .Get<GrpcClientOptions>()!;

            o.Address = new Uri(options.Address);
        });

        services.AddControllers();
        services.AddOpenApi();
        return services;
    }
}