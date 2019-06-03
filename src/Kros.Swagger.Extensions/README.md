# Kros.Swagger.Extensions

**Kros.Swagger.Extensions** je pomocná knižnica pre zjednodušenie práce so Swagger dokumentáciou.

> Swagger slúži na dokumentovanie API rozhrania.

## Extensions

Menný priestor `Kros.Swagger.Extensions` obsahuje rozšírenia pre Swagger.

Registrovaním `services.AddSwaggerDocumentation(Configuration)` pridáte do pipeline-y Swagger, ktorý prejde všetky verejné API a vytvorí z nich dokumentáciu.

```CSharp
public virtual void ConfigureServices(IServiceCollection services)
{
	services.AddSwaggerDocumentation(Configuration);
}
```

Ďalej je potrebné pridať middleware zavolaním `app.UseSwaggerDocumentation(Configuration)` v metóde `Configure`.

```CSharp
public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseSwaggerDocumentation(Configuration);
}
```

Swagger dokumentáciu je možné konfigurovať v `appSettings.json` súbore.

Príklad konfigurácie:

```json
"SwaggerDocumentation": {
  "Version": "v1",
  "Title": "Project Service Api",
  "Description": "Web API for SSW.",
  "Contact": {
    "Name": "Kros",
    "Email": "info@kros.sk",
    "Url": "www.kros.sk"
  },
  "Extensions": {
    "TokenUrl": "https://login.site.com/token",
    "OAuthClientId": "kros_postman"
  }
}
```

