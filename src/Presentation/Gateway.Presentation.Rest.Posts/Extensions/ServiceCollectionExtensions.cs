using Gateway.Presentation.Rest.Posts.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Gateway.Presentation.Rest.Posts.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRestPosts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGrpcClient<PostService.Presentation.Grpc.Protos.PostService.PostServiceClient>(o =>
        {
            GrpcClientOptions options = configuration
                .GetRequiredSection("GrpcClients:PostService")
                .Get<GrpcClientOptions>()!;

            o.Address = new Uri(options.Address);
        });

        services.AddControllers();
        services.AddOpenApi();
        return services;
    }
}
