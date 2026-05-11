using PersonDataApp.Domain.Entities;

namespace PersonDataApp.Application.DTOs;

public record PersonDto(
    string DocumentNumber,
    string FirstName,
    string LastName,
    DateTime? BirthDate,
    int? Age,
    string? Address,
    string? Locality,
    string? Phone,
    string? Email,
    DateTime LastQueriedAt,
    bool FromCache
)
{
    public static PersonDto FromEntity(Person p, bool fromCache)
    {
        int? age = null;
        if (p.BirthDate.HasValue)
        {
            var today = DateTime.Today;
            age = today.Year - p.BirthDate.Value.Year;
            if (p.BirthDate.Value.Date > today.AddYears(-age.Value))
                age--;
        }

        return new PersonDto(
            p.DocumentNumber, p.FirstName, p.LastName, p.BirthDate,
            age, p.Address, p.Locality, p.Phone, p.Email,
            p.LastQueriedAt, fromCache
        );
    }
}
