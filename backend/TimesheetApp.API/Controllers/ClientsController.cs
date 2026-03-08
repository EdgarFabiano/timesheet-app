using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetApp.API.DTOs.Clients;
using TimesheetApp.API.Services;

namespace TimesheetApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly ClientService _clientService;

    public ClientsController(ClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClientResponse>>> GetAll()
    {
        var clients = await _clientService.GetAllAsync();
        return Ok(clients);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientResponse>> GetById(Guid id)
    {
        var client = await _clientService.GetByIdAsync(id);
        if (client == null)
            return NotFound();

        return Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<ClientResponse>> Create([FromBody] CreateClientRequest request)
    {
        var created = await _clientService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClientResponse>> Update(Guid id, [FromBody] UpdateClientRequest request)
    {
        var updated = await _clientService.UpdateAsync(id, request);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _clientService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
