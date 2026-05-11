using PersonDataApp.Domain.Entities;

namespace PersonDataApp.Domain.Ports.Output;

public interface IExternalPersonService
{
    Task<Person?> GetByDocumentNumberAsync(string documentNumber);
}
