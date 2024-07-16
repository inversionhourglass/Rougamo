using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace WebApis
{
    public class Issue75
    {
        public OpenApiDocument GetOpenApiDocument()
        {
            var builder = WebApplication.CreateBuilder([]);

            // Add services to the container.

            builder.Services.AddControllers().AddCurrentApplicationPart();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            return app.Services.GetService<ISwaggerProvider>()!.GetSwagger("v1", null, null);
        }
    }
}
