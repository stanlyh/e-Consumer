using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PersonDataApp.Domain.Entities;
using PersonDataApp.Domain.Ports.Output;

namespace PersonDataApp.Infrastructure.Persistence;

public class PersonRepository(IConfiguration configuration) : IPersonRepository
{
    private SqlConnection CreateConnection() =>
        new(configuration.GetConnectionString("DefaultConnection"));

    public async Task<Person?> FindByDocumentNumberAsync(string documentNumber)
    {
        const string sql = """
            SELECT Id, DocumentNumber, FirstName, LastName, BirthDate,
                   Address, Locality, Phone, Email, LastQueriedAt, CreatedAt, UpdatedAt
            FROM PersonCache
            WHERE DocumentNumber = @DocumentNumber
            """;

        await using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Person>(sql, new { DocumentNumber = documentNumber });
    }

    public async Task UpsertAsync(Person person)
    {
        const string sql = """
            MERGE PersonCache AS target
            USING (SELECT @DocumentNumber AS DocumentNumber) AS source
            ON target.DocumentNumber = source.DocumentNumber
            WHEN MATCHED THEN
                UPDATE SET
                    FirstName     = @FirstName,
                    LastName      = @LastName,
                    BirthDate     = @BirthDate,
                    Address       = @Address,
                    Locality      = @Locality,
                    Phone         = @Phone,
                    Email         = @Email,
                    LastQueriedAt = @LastQueriedAt,
                    UpdatedAt     = GETUTCDATE()
            WHEN NOT MATCHED THEN
                INSERT (DocumentNumber, FirstName, LastName, BirthDate,
                        Address, Locality, Phone, Email, LastQueriedAt)
                VALUES (@DocumentNumber, @FirstName, @LastName, @BirthDate,
                        @Address, @Locality, @Phone, @Email, @LastQueriedAt);
            """;

        await using var conn = CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            person.DocumentNumber,
            person.FirstName,
            person.LastName,
            person.BirthDate,
            person.Address,
            person.Locality,
            person.Phone,
            person.Email,
            person.LastQueriedAt
        });
    }
}
