# Kros.ApplicationInsights.Extensions

**Kros.ApplicationInsights.Extensions** je knižnica, ktorá umožnuje registrovať Application Insights Telemetry.

Registrovaním `services.AddApplicationInsights(IConfiguration Configuration)` inicializujete Application Insights Telemetry.
Pre inicializáciu je potrebné definovať v konfigurácii:

```Json
"ApplicationInsights": {
    "ServiceName": "",
    "ConnectionString": "",
    "SamplingRate": 100
  },
```

`SamplingRate` je percento (100 = všetka telemetria, 50 = polovica, ...). Interne sa prepočíta na `SamplingRatio` (0 - 1).

Ak potrebujete namiesto fixného vzorkovania obmedzenie počtom, nastavte `AdaptiveSamplingOptions`:

```Json
"ApplicationInsights": {
    "ServiceName": "",
    "AdaptiveSamplingOptions": {
      "MaxTelemetryItemsPerSecond": 5
    }
  },
```

Použije sa `TracesPerSecond` (rate limited sampling). Nastavené sú buď `AdaptiveSamplingOptions`, alebo `SamplingRate` -
ak sú vyplnené obidve, prednosť má `AdaptiveSamplingOptions`.

Registráciou sa nastaví vzorkovanie, názov služby a pridajú sa nasledovné procesory.

## Názov služby (predtým `CloudRoleNameInitializer`)

Hodnota `ServiceName` sa zapíše do OpenTelemetry resource ako `service.name` a `service.instance.id`. Z nich Application
Insights odvodí `Cloud.RoleName` a `Cloud.RoleInstance`, takže službu naďalej vidíte napr. v Application Map.

## UserIdFromUserAgentProcessor

Ak je v requeste header `User-Agent`, jeho hodnota sa nastaví do atribútu `enduser.id`, ktorý sa exportuje ako `ai.user.id`.

## RoutePatternProcessor

Ak má používateľ claim `route_pattern`, jeho hodnota sa pridá do telemetrie requestu.

## FilterRequestsProcessor

Requesty na `/health` a `/signalR`, requesty s metódou `OPTIONS` a requesty z Postmana sa nezahrnú do telemetrie.

## FilterSyntheticRequestsProcessor

Preskočí syntetické requesty - requesty od botov, z web searchu a pod.

## HeadersTelemetryProcessor

Umožní logovať ľubovoľné hlavičky z requestu.

```csharp
services.AddHeadersTelemetryProcessor("my-custom-header-1", "my-custom-header-2");
```

Hlavička bude pridaná do properties s kľúčom `Header-{headerKey}`. Pokiaľ chcete tento názov zmeniť, použite na to property name resolver.

```csharp
services.AddHeadersTelemetryProcessor((headerKey) => $"MyPrefix-{headerKey}-myPostfix","my-custom-header-1", "my-custom-header-2");
```

## FilterSensitiveTraceTelemetryProcessor

Nie je registrovaný automaticky, zaregistrujete ho cez `services.AddSensitiveTraceTelemetryProcessor()`.

Ak log začína výpisom hlavičiek `Authorization` alebo `x-functions-key`, jeho správa aj atribúty sa nahradia hodnotou
`FilterSensitiveTraceTelemetryProcessor.RedactedMessage`. Citlivé údaje sa tak neodošlú do Application Insights.

## Migrácia na verziu 5.0.0

Verzia 5.0.0 je postavená na `Microsoft.ApplicationInsights.AspNetCore` 3.x, ktorý je prepísaný na OpenTelemetry.
Pôvodné rozhrania `ITelemetryInitializer` a `ITelemetryProcessor` v ňom už neexistujú, preto sú všetky triedy prepísané
na OpenTelemetry procesory (`BaseProcessor<Activity>`, resp. `BaseProcessor<LogRecord>`).

Zmeny, ktoré je potrebné spraviť v aplikáciách:

- `app.UseApplicationInsights(Configuration)` už nie je potrebné volať. Metóda je označená ako zastaraná, nič nerobí
  a vzorkovanie sa nastavuje pri registrácii služieb.
- `services.AddHeadersTelemetryInitializer(...)` sa premenoval na `services.AddHeadersTelemetryProcessor(...)`.
- `FilterSensitiveTraceTelemetryProcessor` sa registruje cez `services.AddSensitiveTraceTelemetryProcessor()`.
  Namiesto zahodenia telemetrie sa citlivá správa prepíše, lebo OpenTelemetry neumožňuje zahodiť log record.
- `AdaptiveSamplingOptions.ExcludedTypes` a `IncludedTypes` sa už nepoužívajú. Application Insights SDK 3.x neumožňuje
  vzorkovanie podľa typu telemetrie, hodnoty sú ignorované.
- Triedy `CloudRoleNameInitializer`, `UserIdFromUserAgentInitializer`, `RoutePatternInitializer`
  a `HeadersTelemetryInitializer` už neexistujú.
