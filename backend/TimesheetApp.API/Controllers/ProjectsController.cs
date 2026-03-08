using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetApp.API.DTOs.Projects;
using TimesheetApp.API.Services;

namespace TimesheetApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> GetAll()
    {
        var projects = await _projectService.GetAllAsync();
        return Ok(projects);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> GetById(Guid id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null)
            return NotFound();

        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create([FromBody] CreateProjectRequest request)
    {
        var created = await _projectService.CreateAsync(request);
        if (created == null)
            return NotFound("Client not found.");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> Update(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var (project, clientNotFound) = await _projectService.UpdateAsync(id, request);

        if (clientNotFound)
            return NotFound("Client not found.");

        if (project == null)
            return NotFound();

        return Ok(project);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _projectService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

