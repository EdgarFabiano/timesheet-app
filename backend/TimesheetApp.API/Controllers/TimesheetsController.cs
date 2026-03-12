using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetApp.API.DTOs.Timesheets;
using TimesheetApp.API.Services;

namespace TimesheetApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimesheetsController : ControllerBase
{
    private readonly TimesheetService _timesheetService;

    public TimesheetsController(TimesheetService timesheetService)
    {
        _timesheetService = timesheetService;
    }

    /// <summary>Get timesheets. Filter by employeeId + startDate/endDate, or by projectId (optional startDate/endDate).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TimesheetResponse>>> GetAll(
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] Guid? projectId)
    {
        if (employeeId.HasValue)
        {
            if (!startDate.HasValue || !endDate.HasValue)
                return BadRequest("When filtering by employeeId, both startDate and endDate are required.");
            if (startDate.Value > endDate.Value)
                return BadRequest("startDate must be before or equal to endDate.");

            var timesheets = await _timesheetService.GetByEmployeeAndDateRangeAsync(
                employeeId.Value,
                startDate.Value,
                endDate.Value);
            return Ok(timesheets);
        }

        if (projectId.HasValue)
        {
            var timesheets = await _timesheetService.GetByProjectIdAsync(
                projectId.Value,
                startDate,
                endDate);
            return Ok(timesheets);
        }

        return BadRequest("Provide employeeId with startDate/endDate, or projectId to list timesheets.");
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TimesheetResponse>> GetById(Guid id)
    {
        var timesheet = await _timesheetService.GetByIdAsync(id);
        if (timesheet == null)
            return NotFound();

        return Ok(timesheet);
    }

    /// <summary>Submit hours (create timesheet entry). HoursWorked must be between 0.5 and 24.</summary>
    [HttpPost]
    public async Task<ActionResult<TimesheetResponse>> Create([FromBody] CreateTimesheetRequest request)
    {
        var (timesheet, errorKey) = await _timesheetService.CreateAsync(request);

        if (errorKey == "EmployeeNotFound")
            return NotFound("Employee not found.");
        if (errorKey == "ProjectNotFound")
            return NotFound("Project not found.");
        if (errorKey == "DuplicateEntry")
            return Conflict(new { message = "A timesheet entry already exists for this employee, project, and date." });

        return CreatedAtAction(nameof(GetById), new { id = timesheet!.Id }, timesheet);
    }

    /// <summary>Update hours and/or notes. HoursWorked must be between 0.5 and 24.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TimesheetResponse>> Update(Guid id, [FromBody] UpdateTimesheetRequest request)
    {
        var (timesheet, notFound) = await _timesheetService.UpdateAsync(id, request);
        if (notFound)
            return NotFound();

        return Ok(timesheet);
    }

    /// <summary>Delete a timesheet entry.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _timesheetService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
