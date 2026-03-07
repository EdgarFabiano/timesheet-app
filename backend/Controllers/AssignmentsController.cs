using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetApp.API.DTOs.Assignments;
using TimesheetApp.API.Services;

namespace TimesheetApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly AssignmentService _assignmentService;

    public AssignmentsController(AssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    /// <summary>Get assignments. Filter by projectId and/or employeeId (optional).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssignmentResponse>>> GetAll(
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId)
    {
        if (projectId.HasValue && employeeId.HasValue)
        {
            var assignments = await _assignmentService.GetByProjectAndEmployeeAsync(
                projectId.Value,
                employeeId.Value);
            return Ok(assignments);
        }

        if (projectId.HasValue)
        {
            var assignments = await _assignmentService.GetByProjectIdAsync(projectId.Value);
            return Ok(assignments);
        }

        if (employeeId.HasValue)
        {
            var assignments = await _assignmentService.GetByEmployeeIdAsync(employeeId.Value);
            return Ok(assignments);
        }

        return BadRequest("Provide projectId and/or employeeId to list assignments.");
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssignmentResponse>> GetById(Guid id)
    {
        var assignment = await _assignmentService.GetByIdAsync(id);
        if (assignment == null)
            return NotFound();

        return Ok(assignment);
    }

    /// <summary>Assign an employee to a project.</summary>
    [HttpPost]
    public async Task<ActionResult<AssignmentResponse>> Create([FromBody] CreateAssignmentRequest request)
    {
        var (assignment, errorKey) = await _assignmentService.CreateAsync(request);

        if (errorKey == "EmployeeNotFound")
            return NotFound("Employee not found.");
        if (errorKey == "ProjectNotFound")
            return NotFound("Project not found.");
        if (errorKey == "AlreadyAssigned")
            return Conflict(new { message = "Employee is already assigned to this project." });

        return CreatedAtAction(nameof(GetById), new { id = assignment!.Id }, assignment);
    }

    /// <summary>Unassign (remove assignment by id).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _assignmentService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
