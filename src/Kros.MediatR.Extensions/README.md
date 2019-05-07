# Kros.MediatR.Extensions

**Kros.MediatR.Extensions** je knižnica, ktorá rozširuje knižnicu [MediatR](https://github.com/jbogard/MediatR).

## NullCheckPostProcessor

Registrovaním `services.AddMediatRNullCheckPostProcessor()` pridáte do MediatR pipeline-y post processor,
ktorý kontroluje výsledky všetkých requestov a ak je `null` tak vyvolá `NotFoundException`.

Ak potrebujete niektoré requesty vynechať z kontrolovania, môžete tak spraviť nasledovne:

```CSharp
services.AddMediatRNullCheckPostProcessor((o) =>
{
   o.IgnoreRequest<CreateDefaultUserCommand>();
   o.IgnoreRequest<AnotherCommand>();
});
```

## ControllerBaseExtensions

Rozširuje `ControllerBase` o veci, ktoré zjednodušujú prácu s MediatR v controllery.

Napríklad: zavolať požiadavku cez MediatR môžte nasledovne:

```CSharp
public async Task<GetToDoQuery.ToDo> GetToDo(int id)
  => await this.SendRequest(new GetToDoQuery(id));
```

## Registrovanie pipeline behaviors na základe rozhraní

Umožňuje registrovať pipeline behaviors pre všetky requesty, ktoré implementujú požadované rozhrania.

Napríklad:

```CSharp
services.AddPipelineBehaviorsForRequest<IUserResourceQuery>()
```
