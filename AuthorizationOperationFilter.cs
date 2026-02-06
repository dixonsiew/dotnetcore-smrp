using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

// https://github.com/domaindrivendev/swashbuckle.aspnetcore/issues/3656
namespace smrp
{
    public class AuthorizationOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var actionMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
            var isAuthorized = actionMetadata.Any(metadataItem => metadataItem is AuthorizeAttribute);
            var allowAnonymous = actionMetadata.Any(metadataItem => metadataItem is AllowAnonymousAttribute);
            if (!isAuthorized || allowAnonymous)
            {
                return;
            }

            operation.Parameters ??= new List<IOpenApiParameter>();
            operation.Security =
            [
                new() {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", context.Document),
                        new List<string>()
                    }
                },
            ];
        }
    }
}
