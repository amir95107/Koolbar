using Koolbar.Repositories;

namespace Koolbar.Services;
public static class RegisterServices
{
    public static void Handle(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRequestRepository, RequestRepository>();
        builder.Services.AddScoped<IStateRepository, StateRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        
    }
}