using PersonDataApp.Application.DTOs;

namespace PersonDataApp.Application.Ports;

public interface IGetPersonUseCase
{
    Task<PersonDto?> ExecuteAsync(string documentNumber);
}
