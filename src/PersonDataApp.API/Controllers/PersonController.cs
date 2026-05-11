using Microsoft.AspNetCore.Mvc;
using PersonDataApp.Application.Ports;

namespace PersonDataApp.API.Controllers;

[ApiController]
[Route("api/persons")]
public class PersonController(IGetPersonUseCase useCase) : ControllerBase
{
    [HttpGet("{documentNumber}")]
    public async Task<IActionResult> GetPerson(string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
            return BadRequest("El número de documento es requerido.");

        var result = await useCase.ExecuteAsync(documentNumber.Trim());
        return result is null ? NotFound() : Ok(result);
    }
}
