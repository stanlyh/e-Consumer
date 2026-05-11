using PersonDataApp.Domain.Entities;
using PersonDataApp.Domain.Ports.Output;

namespace PersonDataApp.Infrastructure.ExternalServices;

public class FakePersonServiceAdapter : IExternalPersonService
{
    private static readonly string[] FirstNames = ["Carlos", "María", "Jorge", "Ana", "Luis", "Sofía"];
    private static readonly string[] LastNames  = ["González", "Rodríguez", "Martínez", "López", "García"];
    private static readonly string[] Localities = ["Buenos Aires", "Córdoba", "Rosario", "Mendoza", "La Plata"];

    public Task<Person?> GetByDocumentNumberAsync(string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
            return Task.FromResult<Person?>(null);

        var seed   = documentNumber.GetHashCode();
        var rng    = new Random(seed);
        var first  = FirstNames[rng.Next(FirstNames.Length)];
        var last   = LastNames[rng.Next(LastNames.Length)];
        var city   = Localities[rng.Next(Localities.Length)];

        var person = new Person
        {
            DocumentNumber = documentNumber,
            FirstName      = first,
            LastName       = last,
            BirthDate      = new DateTime(1960 + rng.Next(40), rng.Next(1, 13), rng.Next(1, 28)),
            Address        = $"Av. {last} {rng.Next(100, 9999)}",
            Locality       = city,
            Phone          = $"+54 11 {rng.Next(1000, 9999)}-{rng.Next(1000, 9999)}",
            Email          = $"{first.ToLower()}.{last.ToLower()}@example.com",
            LastQueriedAt  = DateTime.UtcNow
        };

        return Task.FromResult<Person?>(person);
    }
}
