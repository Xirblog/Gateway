using Gateway.Presentation.Rest.Auth.Extensions;
using Gateway.Presentation.Rest.Posts.Extensions;
using Gateway.Presentation.Rest.Users.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHttpContextAccessor()
    .AddRestAuth(builder.Configuration)
    .AddRestUsers(builder.Configuration)
    .AddRestPosts(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();