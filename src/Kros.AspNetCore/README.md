# Kros.AspNetCore

**Kros.AspNetCore** je univerzálna knižnica obsahujúca nástroje na zjednodušenie práce na Asp.Net Core web api projektoch.

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
  
  Obsahuje nastavenie `CORS` policy. Je možné povoliť všetky domény pomocou `AddAllowAnyOriginCors`, alebo povoliť iba vymenované domény pomocou metódy `AddCustomOriginsCorsPolicy`. Tieto domény je potrebné vymenovať v `appsettings.json` v sekcii `AllowesHosts`.

## BaseStartup

Základná `Startup` trieda obsahujúca nastavenie `appsettings.json` a konfiguráciu `CORS` policy. V `development` režime sú pre `CORS ` povolené všetky domény.
