# Kros.AspNetCore

**Kros.AspNetCore** je univerzálna knižnica obsahujúca nástroje na zjednodušenie práce na Asp.Net Core web api projektoch.

- [Kros.AspNetCore](#KrosAspNetCore)
  - [Exceptions](#Exceptions)
  - [Middlewares](#Middlewares)
  - [Extensions](#Extensions)
  - [BaseStartup](#BaseStartup)
  - [Authorization](#Authorization)
  - [JsonPatchDocumentExtensions](#JsonPatchDocumentExtensions)
    - [Flattening pattern](#Flattening-pattern)
    - [Custom mapping](#Custom-mapping)

## Exceptions

Menný priestor `Kros.AspNetCore.Exceptions` obsahuje výnimky, ktoré reprezetentujú Http chybové stavy.

> Napríklad: ak v nejakej triede (typicky servisné triedy) potrebujete propagovať informáciu o chýbajúcom zdroji na ktorý sa dotazujeme, môžete vyvolať výnimku `NotFoundException`.

## Middlewares

Menný priestor `Kros.AspNetCore.Middlewares` obsahuje užitočné middlewares, ktoré je možné pridať do pipeliny a riešia štandardné veci.

> Napríklad: zavolaním `app.UseErrorHandling()` pridáme middleware `ErrorHandlingMiddleware`, ktorý spracuje výnimky z `Kros.AspNetCore.Exception` a pretransformuje ich na prisluchajúci Http status kód.

## Extensions

V adresári `Extensions` sa nachádzajú rôzne rozšírenia štandardných tried (rozhraní) v Asp.Net Core, ktoré zjednodušujú prácu s nimi.

- ### ConfigurationExtensions

  Jednoduchšie získavanie jednotlivých nastavení z konfigurácie.

- ### DistributedCacheExtensions

  Jednoduchšie vkládanie/získavanie komplexných typov z/do distribuovanej keše.

  Taktiež umožňuje získať hodnotu z keše a keď tam nieje tak ju vytvoriť a do keše vložiť.

  ```CSharp
  var toDo = await _cache.GetAndSetAsync(
     "toDo:1",
     () => _database.Query().FirstOrDefault(t=> t.Id == 1),
     options);
  ```

- ### ServiceCollectionExtensions

  Obsahuje jednoduchšie konfigurovanie options do DI kontajnera. A odľahčené registrovanie Mvc pre web api - `services.AddWebApi()`

- ### CorsExtensions

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
