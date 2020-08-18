# Kros.ApplicationInsights.Extensions

**Kros.ApplicationInsights.Extensions** je knižnica, ktorá umožnuje registrovať Application Insights Telemetry.


Registrovaním `services.AddApplicationInsights(IConfiguration Configuration)` a `app.UseApplicationInsights(IConfiguration Configuration)`,
inicializujete Application Insigts Telemetry. Pre inicializáciu je potrebné definovať v konfigurácii:

```Json
"ApplicationInsights": {
    "ServiceName": "",
    "InstrumentationKey": "",
    "SamplingRate": ""
  },
```

SamplingRate je číslo 100/N, kde N je integer, t.j. prípustné hodnoty pre SamplingRate sú 100, 50, 25, 10, 1, 0,1.

Registráciou sa vypne AdaptiveSampling, nastaví sa FixedRateSampling s hodnotou SamplingRate, a pridajú sa následné inštancie `ITelemetryInitializer` a `ITelemetryProcessor`.


## CloudRoleNameInitializer

Nastaví názov pre service v AI. Pod týmto názvom môžeme service vidieť napr. v Application Map.

## UserIdFromUserAgentInitializer

Ak je v requeste header `User-Agent`,  jeho hodnota sa nastaví v telemetry do `Context.User.Id`.


## FilterRequestsProcessor

Ak sa jedná o request na url `/health`, request sa nezahrnie do telemetry.

## FilterSyntheticRequestsProcessor

Preskoćí syntetické requesty - requesty od botov, z web searchu a pod.