using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetApp.API.DTOs.Employees;
using TimesheetApp.API.Services;

namespace TimesheetApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;

    public EmployeesController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmployeeResponse>>> GetAll()
    {
        var employees = await _employeeService.GetAllAsync();
        return Ok(employees);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeResponse>> GetById(Guid id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeResponse>> Create([FromBody] CreateEmployeeRequest request)
    {
        var (created, conflictKey) = await _employeeService.CreateAsync(request);
        if (conflictKey != null)
            return Conflict(new { duplicateField = conflictKey });

        return CreatedAtAction(nameof(GetById), new { id = created!.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeResponse>> Update(Guid id, [FromBody] UpdateEmployeeRequest request)
    {
        var (updated, conflictKey) = await _employeeService.UpdateAsync(id, request);
        if (conflictKey != null)
            return Conflict(new { duplicateField = conflictKey });
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _employeeService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
