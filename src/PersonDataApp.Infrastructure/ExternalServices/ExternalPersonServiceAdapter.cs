using System.Text.Json;
using Microsoft.Extensions.Logging;
using PersonDataApp.Domain.Entities;
using PersonDataApp.Domain.Ports.Output;

namespace PersonDataApp.Infrastructure.ExternalServices;

public class ExternalPersonServiceAdapter(
    HttpClient httpClient,
    ILogger<ExternalPersonServiceAdapter> logger) : IExternalPersonService
{
    public async Task<Person?> GetByDocumentNumberAsync(string documentNumber)
    {
        try
        {
            var response = await httpClient.GetAsync($"persons/{documentNumber}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new Person
            {
                DocumentNumber = documentNumber,
                FirstName      = root.GetPropertyOrDefault("firstName"),
                LastName       = root.GetPropertyOrDefault("lastName"),
                BirthDate      = root.TryGetProperty("birthDate", out var bd) && bd.ValueKind != JsonValueKind.Null
                                    ? bd.GetDateTime()
                                    : null,
                Address        = root.GetPropertyOrDefault("address"),
                Locality       = root.GetPropertyOrDefault("locality"),
                Phone          = root.GetPropertyOrDefault("phone"),
                Email          = root.GetPropertyOrDefault("email"),
                LastQueriedAt  = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error querying external service for document {DocumentNumber}", documentNumber);
            return null;
        }
    }
}

file static class JsonElementExtensions
{
    public static string GetPropertyOrDefault(this JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null
            ? prop.GetString() ?? string.Empty
            : string.Empty;
}
