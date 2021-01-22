# Kros.ProblemDetails.Extensions

**Kros.ProblemDetails.Extensions** je pomocná knižnica, ktorá rozširuje knižnicu [ProblemDetails](https://github.com/khellang/Middleware).

## Extensions

Menný priestor `Kros.ProblemDetails.Extensions` obsahuje rozšírenia pre ProblemDetails.

Registrovaním `services.AddKrosProblemDetails()` pridáte do pipeline-y nastavenie ProblemDetails do HTTP response.
Registrácia nastaví pre [validačnú chybu](https://github.com/FluentValidation/FluentValidation/blob/master/src/FluentValidation/ValidationException.cs) z fluent validácii response code 400 a podrobnejší ProblemDetail.

```CSharp
public virtual void ConfigureServices(IServiceCollection services)
{
	services.AddKrosProblemDetails();
}
```

Je možné si dokonfigurovať vlastné nastavenia pre ProblemDetails. Napríklad nastaviť svoje vlastné ProblemDetails pre ďalšiu chybu. 
```CSharp
public virtual void ConfigureServices(IServiceCollection services)
{
	services.AddKrosProblemDetails(p => p.Map<YourCustomException>(SetYourCustomProblemDetails));
}
```

Ďalej je potrebné pridať middleware zavolaním `app.UseProblemDetails()` z pôvodného nugetu [ProblemDetails](https://github.com/khellang/Middleware) v metóde `Configure`.

```CSharp
public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseProblemDetails();
}
```
