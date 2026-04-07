using Gateway.Presentation.Rest.Users.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;

namespace Gateway.Presentation.Rest.Users.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRestUsers(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddGrpcClient<UserService.Presentation.Grpc.Protos.UserService.UserServiceClient>(o =>
            {
                GrpcClientOptions options = configuration
                    .GetRequiredSection("GrpcClients:UserService")
                    .Get<GrpcClientOptions>()!;

                o.Address = new Uri(options.Address);
            })
            .ConfigureChannel(o => o.UnsafeUseInsecureChannelCallCredentials = true)
            .AddCallCredentials((context, metadata, serviceProvider) =>
            {
                IHttpContextAccessor httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                string? userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userId))
                {
                    metadata.Add("x-user-id", userId);
                }

                return System.Threading.Tasks.Task.CompletedTask;
            });

        services.AddControllers();
        services.AddOpenApi();
        return services;
    }
}
