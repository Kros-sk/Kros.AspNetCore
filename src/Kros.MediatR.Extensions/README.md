# Kros.MediatR.Extensions

__Kros.MediatR.Extensions__ je knižnica, ktorá rozširuje knižnicu [MediatR]([https://link](https://github.com/jbogard/MediatR)).

### NullCheckPostProcessor
Registrovaním `service.AddMediatRNullCheckPostProcessor()` pridáte do MediatR pipeline-y post processor, ktorý kontroluje výsledok requestu a ak je `null` tak vyvolá `NotFoundException`.

### ControllerBaseExtensions
Rozširuje `ControllerBase` o veci, ktoré zjednodušujú prácu s MediatR v controllery.
>Napríklad: zavolať požiadavku cez MediatR môžte nasledovne:
>```CSharp
>public async Task<GetToDoQuery.ToDo> GetToDo(int id)
>  => await this.SendRequest(new GetToDoQuery(id));
>```

### Registrovanie pipeline behaviours na základe rozhraní
Umožňuje registrovať pipeline behaviours pre všetky requesty, ktoré implementujú požadované rozhrania.
> Napríklad: `AddPipelineBehaviorsForRequest<IUserResourceQuery, IUserResourceQueryResult>()`