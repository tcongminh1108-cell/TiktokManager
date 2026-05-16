using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace TikTokShop.Api.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiWithJwt(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "TikTok Shop Management API",
                    Version = "v1",
                    Description = "Multi-tenant TikTok Shop management — inventory, orders, analytics."
                };
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter JWT access token (no 'Bearer' prefix needed)"
                };
                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, ct) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                if (metadata.OfType<AuthorizeAttribute>().Any()
                    && !metadata.OfType<AllowAnonymousAttribute>().Any())
                {
                    operation.Security =
                    [
                        new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            }] = []
                        }
                    ];
                }
                return Task.CompletedTask;
            });
        });

        return services;
    }
}
