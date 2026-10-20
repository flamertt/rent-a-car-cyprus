using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace RentACar.Api.OpenApi;

/// <summary>OpenAPI belgesine JWT Bearer güvenlik şemasını ekler (Scalar'da "Authorize" ile belirteç girilir).</summary>
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public const string SchemeId = "Bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "POST /api/auth/login yanıtındaki accessToken.",
        };
        return Task.CompletedTask;
    }
}
