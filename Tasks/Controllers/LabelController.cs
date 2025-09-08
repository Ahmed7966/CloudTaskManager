using CloudTaskManager.Data;
using CloudTaskManager.DTO_s;
using CloudTaskManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudTaskManager;

[Authorize]
[ApiController]
[Route("api/labels")]
public class LabelController(TaskDbContext taskDbContext) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateLabel(CreateLabelDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var label = new Label { Name = dto.Name, Color = dto.Color };
        await taskDbContext.Labels.AddAsync(label);
        await taskDbContext.SaveChangesAsync();

        return Ok(label);
    }

    [HttpGet]
    public async Task<IActionResult> GetLabels()
    {
        var labels = await taskDbContext.Labels.ToListAsync();
        return Ok(labels);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateLabel(UpdateLabelDto dto)
    {
        var label = await taskDbContext.Labels.FindAsync(dto.Id);
        if (label == null) return NotFound("Label not found");

        if (!string.IsNullOrEmpty(dto.Name)) label.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Color)) label.Color = dto.Color;

        await taskDbContext.SaveChangesAsync();
        return Ok(new { label.Id, Message = "Label updated successfully" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteLabel(Guid id)
    {
        var label = await taskDbContext.Labels.FindAsync(id);
        if (label == null) return NotFound("Label not found");

        taskDbContext.Labels.Remove(label);
        await taskDbContext.SaveChangesAsync();
        return NoContent();
    }
}