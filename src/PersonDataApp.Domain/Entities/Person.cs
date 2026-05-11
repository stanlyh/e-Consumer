namespace PersonDataApp.Domain.Entities;

public class Person
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string? Address { get; set; }
    public string? Locality { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime LastQueriedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsCacheStale(int maxAgeDays = 20) =>
        LastQueriedAt < DateTime.UtcNow.AddDays(-maxAgeDays);
}
