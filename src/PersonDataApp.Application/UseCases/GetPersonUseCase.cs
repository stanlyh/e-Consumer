using PersonDataApp.Application.DTOs;
using PersonDataApp.Application.Ports;
using PersonDataApp.Domain.Ports.Output;

namespace PersonDataApp.Application.UseCases;

public class GetPersonUseCase(
    IPersonRepository repository,
    IExternalPersonService externalService) : IGetPersonUseCase
{
    public async Task<PersonDto?> ExecuteAsync(string documentNumber)
    {
        var cached = await repository.FindByDocumentNumberAsync(documentNumber);

        if (cached is not null && !cached.IsCacheStale())
            return PersonDto.FromEntity(cached, fromCache: true);

        var person = await externalService.GetByDocumentNumberAsync(documentNumber);
        if (person is null)
            return null;

        person.LastQueriedAt = DateTime.UtcNow;
        await repository.UpsertAsync(person);

        return PersonDto.FromEntity(person, fromCache: false);
    }
}
