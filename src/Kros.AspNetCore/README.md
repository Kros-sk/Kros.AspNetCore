# Kros.AspNetCore

**Kros.AspNetCore** je univerzálna knižnica obsahujúca nástroje na zjednodušenie práce na Asp.Net Core web api projektoch.

- [Kros.AspNetCore](#krosaspnetcore)
  - [Exceptions](#exceptions)
  - [Middlewares](#middlewares)
  - [Extensions](#extensions)
    - [Configuration](#configuration)
      - [Príklad](#pr%c3%adklad)
    - [ConfigurationBuilderExtensions](#configurationbuilderextensions)
      - [Príklad](#pr%c3%adklad-1)
    - [DistributedCacheExtensions](#distributedcacheextensions)
    - [CorsExtensions](#corsextensions)
  - [BaseStartup](#basestartup)
  - [Authorization](#authorization)
  - [JsonPatchDocumentExtensions](#jsonpatchdocumentextensions)
    - [Flattening pattern](#flattening-pattern)
    - [Custom mapping](#custom-mapping)
  - [Service Discovery Provider](#service-discovery-provider)
    - [Get started](#get-started)
    - [GatewayAuthorizationMiddleware](#gatewayauthorizationmiddleware)

## Exceptions

Menný priestor `Kros.AspNetCore.Exceptions` obsahuje výnimky, ktoré reprezetentujú Http chybové stavy.

> Napríklad: ak v nejakej triede (typicky servisné triedy) potrebujete propagovať informáciu o chýbajúcom zdroji na ktorý sa dotazujeme, môžete vyvolať výnimku `NotFoundException`.

## Middlewares

Menný priestor `Kros.AspNetCore.Middlewares` obsahuje užitočné middlewares, ktoré je možné pridať do pipeliny a riešia štandardné veci.

> Napríklad: zavolaním `app.UseErrorHandling()` pridáme middleware `ErrorHandlingMiddleware`, ktorý spracuje výnimky z `Kros.AspNetCore.Exception` a pretransformuje ich na prisluchajúci Http status kód.

## Extensions

V adresári `Extensions` sa nachádzajú rôzne rozšírenia štandardných tried (rozhraní) v Asp.Net Core, ktoré zjednodušujú prácu s nimi.

### Configuration

Na jednoduchú konfiguráciu slúži extension metóda `ConfigureOptions<TOptions>(Configuration)`. Táto metóda triedu `TOptions`
napojí na nastavenia (konfigurácia napríklad v `appsettings.json`) podľa názvu triedy a triedu zaregistruje do DI kontajnera ako:

- `IOptions<TOptions>`, `IOptionsSnapshot<TOptions>` (štandardná registrácia metódou `Configure`)
- `TOptions`

Keďže je trieda zaregistrovaná aj priamo, v iných triedach je možné ju priamo používať ako závislosť (tzn. nie je potrebné
používať komplikovanejšiu závislosť na `IOptions<TOptions>`). Trieda je registrovaná ako `Transient` a podporuje _hot reload_.
Takže ak sa zmenia nastavenia, ďalšia inštancia `TOptions` vypýtaná z DI kontajnera bude mať nové nastavenia.

Nastavenia v konfigurácii sa hľadajú podľa názvu triedy, pričom ak trieda má koncovku `Options`, alebo `Settings`, táto
koncovka ignoruje. Tzn. trieda `SmtpSettings` alebo `SmtpOptions` je nastavená podľa sekcie s názvom `Smtp`.

Konfiguráciu je možné validovať implementovaním rozhrania `IValidatable` v triede nastavení. Rozhranie má jedinú metódu
`Validate()`, ktorá vykonáva validáciu. V prípade chyby validácia vyvolá ľubovoľnú výnimku, pričom preferovaná je výnimka
`SettingsValidationException`. Pre jednoduchšiu implementáciu nastavení ktoré sa validujú je vytvorená základná trieda
`AnnotatedSettingsBase`, ktorá implementuje validáciu pomocou *data annotations* atribútov. Takže samotnú validáciu nie je
potrebné implementovať, akurát príslušnými atribútmi odekorovať potrebné vlastnosti. _(Ak validácia zlyhá, nie je vyvolaná
výnimka `SettingsValidationException`, ale `ValidationException` z data annotácií.)_

Aby sa samotná validácia spustila, je potrebné v `ConfigureServices` zaregistrovať validačný `IStartupFilter` volaním
extension metódy `UseConfigurationValidation()`. Validácia prebehne jednorázovo po spustení aplikácie, ale ešte pred tým,
než sú obsluhované požiadavky. Pomocou validácie je možné vynútiť základné nutné nastavenia, bez ktorých nemá spustenie
aplikácie zmysel.

#### Príklad

``` csharp
public class SmtpSettings : AnnotatedSettingsBase
{
    // This value is required, so it must be set in configuration file.
    [Required]
    public string Server { get; set; }

    public int Port { get; set; }
}

public class Startup
{
    public virtual void ConfigureServices(IServiceCollection services)
    {
        // Adds SmtpSettings class into DI container and loads the settings from "Smtp" section in configuraion.
        services.ConfigureOptions<SmtpSettings>(Configuration);
        services.AddSingleton<SmtpEmailSender>();

        // During startup, all IValidatable objects in DI container are validated and the startup fails if some
        // validation fails. In this example, it will fail, if "SmtpSettings.Server" is not set in settings.
        services.UseConfigurationValidation();
    }
}

public class SmtpEmailSender
{
    // SmtpSettings is resolved from DI container.
    public SmtpEmailSender(SmtpSettings settings)
    {

    }
}
```
### ConfigurationBuilderExtensions
Na pridanie Azure App Configuration slúži metóda `AddAzureAppConfiguration(HostBuilderContext)`.

#### Príklad
Konfigurácia obsahuje hodhoty:
```json
{
  "AppConfig": {
    "Endpoint": "https://example.azconfig.io",
    "Settings": [ "Example" ]
  }
}
```

Nasledujúci kód pridá konfiguračné hodnoty (ktorých kľúč má predponu "Examle") z Azure App Configuration.

```CSharp
Host.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration((hostingContext, config) =>
	{
		config.AddAzureAppConfiguration(hostingContext);
	});
```


### DistributedCacheExtensions

Jednoduchšie vkladanie/získavanie komplexných typov z/do distribuovanej keše.

Taktiež umožňuje získať hodnotu z keše a keď tam nieje tak ju vytvoriť a do keše vložiť.

```CSharp
var toDo = await _cache.GetAndSetAsync(
    "toDo:1",
    () => _database.Query().FirstOrDefault(t=> t.Id == 1),
    options);
```

### CorsExtensions

Obsahuje nastavenie `CORS` policy. Je možné povoliť všetky domény pomocou `AddAllowAnyOriginCors`, alebo povoliť iba vymenované domény pomocou metódy `AddCustomOriginsCorsPolicy`. Tieto domény je potrebné vymenovať v `appsettings.json` v sekcii `AllowedHosts`.

## BaseStartup

Základná `Startup` trieda obsahujúca nastavenie `appsettings.json` a konfiguráciu `CORS` policy. V `development` režime sú pre `CORS` povolené všetky domény.

## Authorization

[PR #19](https://github.com/Kros-sk/Kros.AspNetCore/pull/19) adds the ability to authorize internal (downstream) services behind the api gateway.

The Api gateway will use `GatewayAuthorizationMiddleware` middleware to contact the authorization service to forward the `Authorization Header` from the original request. Your authorization service will validate this token and create new credential token to be routed to internal services.

To add middleware, you must first register the related services `services.AddGatewayJwtAuthorization()`
and then register `app.UseGatewayJwtAuthorization()` to the pipeline.

You can use `JwtAuthorizationHelper` to generate a Jwt token.

## JsonPatchDocumentExtensions

This package contains extension for mapping JSON patch operations paths from `JsonPatchDocument<TModel>` class to database columns names.

```CSharp
IEnumerable<Foo> columns = jsonPatch.GetColumnsNames();
```

### Flattening pattern

This extension use flattening pattern for mapping operation path to column name.
For example: path `/Supplier/Name` is maped to `SupplierName`.

### Custom mapping

When you need define custom mapping, you can use `JsonPatchDocumentMapperConfig<TModel>` for configuration.

```CSharp
JsonPatchMapperConfig<Document>
  .NewConfig()
  .Map(src =>
  {
      const string address = "/Address/";

      var index = src.IndexOf(address);
      if (index > -1)
      {
          return src.Remove(index, address.Length);
      }

      return src;
  });
```

```CSharp
var jsonPatch = new JsonPatchDocument<Document>();
jsonPatch.Replace(p => p.Supplier.Address.Country, "Slovakia");
jsonPatch.Replace(p => p.Supplier.Address.PostCode, "0101010");

var columns = jsonPatch.GetColumnsNames();
columns.Should()
  .BeEquivalentTo("SupplierCountry", "SupplierPostCode");
```

If you don't want map a path, then return `null` from mapping function.

```CSharp
JsonPatchMapperConfig<Document>
  .NewConfig()
  .Map(src =>
  {
      if (src.Contains("/Address/"))
      {
          return null;
      }

      return src;
  });
```

## Service Discovery Provider

Častokrát sa stáva, že niektorá služba potrebuje robiť dotaz na inú službu. V taktomto prípade býva niekde v settingoch konfigurácia, ktorá obsahuje adresu danej služby. Keďže už v rámci ApiGateway budeme používať [Service Discovery Provider](https://github.com/Burgyn/MMLib.Ocelot.Provider.AppConfiguration) na definíciu služieb. Tak tieto konfigurácie môžme využiť aj na to aby sme nemuseli zbytočne vo všetkých službách definovať tie isté adresy.

### Get started

1. Definujme si služby

```json
"Services": {
  "organizations": {
    "DownstreamPath": "http://localhost:9003"
  },
  "authorization": {
    "DownstreamPath": "http://localhost:9002",
    "Paths":{
      "jwt": "/api/authorization/jwt-token"
    }
  },
  "toDos": {
    "DownstreamPath": "http://localhost:9001"
  }
}
```

> Ideálne však v Azure AppConfiguration, aby sme jednotlivé definície mohli zdieľať naprieš službami.

2. Pridajme si `IServiceDiscoveryProvider`

```CSharp
services.AddServiceDiscovery();
```

3. Použime `IServiceDiscoveryProvider` injecnutý do vašich tried

```CSharp
provider.GetPath("authorization","jwt");
```

Druhá možnosť ako používať provider na získanie služby, je vytvoriť si ľuboboľný Enum na rozlíšenie služieb, a jednotlivým hodnotám enumu nastaviť atribút `ServiceNameAttribute`

```CSharp
public enum ServiceType
{
    [ServiceName("authorization")]
    Authorization,
    [ServiceName("organizations")]
    Organizations,
    [ServiceName("toDos")]
    ToDos,
        
} 
```

Potom je možné volať získavanie služieb pomocou hodnôt enumu

```CSharp
provider.GetPath(ServiceType.Authorization, "jwt");
```
alebo
```CSharp
provider.GetService(ServiceType.Organizations);
```

### GatewayAuthorizationMiddleware

Už aj `GatewayAuthorizationMiddleware` podporuje `IServiceDiscoveryProvider`

```json
"GatewayJwtAuthorization": {
    "Authorization": {
      "ServiceName": "authorization",
      "PathName": "jwt"
    },
    "HashAuthorization": {
      "ServiceName": "authorization",
      "PathName": "jwt"
    },
    "CacheSlidingExpirationOffset": "00:00:00",
    "IgnoredPathForCache": [
      "/organizations"
    ]
  }
```
