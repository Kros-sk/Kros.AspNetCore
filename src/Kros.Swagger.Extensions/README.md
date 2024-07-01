# Kros.Swagger.Extensions

**Kros.Swagger.Extensions** is helper library for easier work with Swagger documentation.

## Extensions

`Kros.Swagger.Extensions` namespace contains extensions for Swagger.

There are two steps that need to be done to add Swagger documentation to your application.

- Register services by calling `builder.Services.AddSwaggerDocumentation(builder.Configuration)`.
- Add middleware by calling `app.UseSwaggerDocumentation(app.Configuration)`.

Swagger can be configured in `appsettings.json` in `SwaggerDocumentation` section.

```json
"SwaggerDocumentation": {
    "version": "1.0",
    "title": "Payrolls API",
    "description": "KROS Payrolls service API.",
    "contact": {
        "name": "KROS a.s.",
        "email": "info@example.com",
        "url": "https://www.example.com"
    }
}
```

Library supports authorization in Swagger, read more in examples.

## Examples

### Simple usage

```csharp
builder.Services.AddSwaggerDocumentation(builder.Configuration);

...

if (app.Environment.IsTestOrDevelopment())
{
    app.UseSwaggerDocumentation(app.Configuration);
}
```

### Fine grained configuration

```csharp
builder.Services.AddSwaggerDocumentation(builder.Configuration, config =>
{
    // Use 'config' object to configure Swagger.
    // This is standard Swagger configuration when using Swagger directly by 'services.AddSwaggerGen()'.
});

...

if (app.Environment.IsTestOrDevelopment())
{
    app.UseSwaggerDocumentation(app.Configuration,
        setupAction: config =>{
          // Use 'config' object to configure Swagger.
          // This is standard Swagger configuration when using Swagger directly by 'app.UseSwagger'.
        },
        setupUiAction: configUi =>
        {
          // Use 'config' object to configure Swagger UI.
          // This is standard Swagger configuration when using Swagger UI directly by 'app.UseSwaggerUI'.
        }
    );
}
```

### Authorization

To declaratively configure authorization, use `authorizations` property in configuration.
This is an object property, where keys in it are arbitrary name for the authorization.
The authorization configuration the is just `OpenApiSecurityScheme` object. It must have authorization `type`
defined and at least one `flow`.

```json
"SwaggerDocumentation": {
    "version": "1.0",
    "title": "Payrolls API",
    "description": "KROS Payrolls service API.",
    "authorizations": {
        "loginWtf": {
            "type": "oauth2",
            "flows": {
                "password": {
                    "tokenUrl": "https://login.kros.wtf/connect/token",
                    "scopes": {
                        "Kros.Payrolls": "Human description of the scope."
                    }
                }
            }
        }
    }
}
```

To correctly authorize endpoints in Swagger (called operations), the must have correct _security requirement_
defined. There are two confiruration extension methods available for this purpose.

**If by default all your endpoints are authorized**, and only some of them allows anonymous access,
use `ClearSecurityRequirementsFromAnonymousEndpoints` when configurion Swagger services.
This clears security requirements for all endpoints which are marked as anonymous (`AllowAnonymous` attribute),
and sets security requirements for all other endpoints.

**If by default all your endpoints are public**, and only some of them are authorized,
use `SetSecurityRequirementsToAuthorizedEndpoints` when configurion Swagger services.
This sets security requirements for all endpoints which are marked as authorized (`Authorize` attribute),
and clears security requirements for all other endpoints.

This extensions work for both, standard attributes and extension methods in minimal API.

```csharp
builder.Services.AddSwaggerDocumentation(builder.Configuration, config =>
{
    config.ClearSecurityRequirementsFromAnonymousEndpoints();
});
```

> Although the library supports multiple authorizations with multiple flows in configuration,
> this has not been tested.
