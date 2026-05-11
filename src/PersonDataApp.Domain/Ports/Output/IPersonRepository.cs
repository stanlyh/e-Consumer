using PersonDataApp.Domain.Entities;

namespace PersonDataApp.Domain.Ports.Output;

public interface IPersonRepository
{
    Task<Person?> FindByDocumentNumberAsync(string documentNumber);
    Task UpsertAsync(Person person);
}
