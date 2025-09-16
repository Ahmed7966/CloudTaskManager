using CloudTaskManager.Data;
using CloudTaskManager.DTO_s;
using CloudTaskManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudTaskManager.Shared.Correlation;

namespace CloudTaskManager;

[Authorize]
[ApiController]
[Route("api/Board")]
public class BoardController(
    TaskDbContext taskDbContext,
    ILogger<BoardController> logger,
    ICorrelationIdAccessor correlationIdAccessor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBoards()
    {
        var boards = await taskDbContext.Boards.ToListAsync();
        return Ok(boards);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard(BoardDto boardDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning(
                $"Invalid request to create board: {boardDto} [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return BadRequest(ModelState);
        }

        var board = new Board
        {
            Name = boardDto.Name,
            Description = boardDto.Description,
        };
        await taskDbContext.Boards.AddAsync(board);
        await taskDbContext.SaveChangesAsync();
        return Ok(board);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateBoard(UpdateBoardDto updateBoardDto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning(
                $"Invalid request to update board: {updateBoardDto} [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return BadRequest(ModelState);
        }

        var board = await taskDbContext.Boards.FindAsync(updateBoardDto.Id);
        if (board == null)
        {
            logger.LogWarning(
                $"Board with id: {updateBoardDto.Id} does not exist [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return NotFound("Board not found");
        }

        if (!string.IsNullOrEmpty(updateBoardDto.Name))
            board.Name = updateBoardDto.Name;
        if (!string.IsNullOrEmpty(updateBoardDto.Description))
            board.Description = updateBoardDto.Description;
        if (updateBoardDto.TaskIds != null)
        {
            var tasks = await taskDbContext.TaskItems
                .Where(t => updateBoardDto.TaskIds.Contains(t.Id))
                .ToListAsync();
            board.Tasks = tasks;
        }

        if (updateBoardDto.MemberIds != null)
        {
            var members = await taskDbContext.Members
                .Where(m => updateBoardDto.MemberIds.Contains(m.Id))
                .ToListAsync();
            board.Members = members;
        }

        await taskDbContext.SaveChangesAsync();
        logger.LogInformation(
            $"Board with id: {updateBoardDto.Id} has been updated [CorrelationId: {correlationIdAccessor.CorrelationId}]");
        return Ok(board);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoard(int id)
    {
        var deletedCount = await taskDbContext.Boards
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync();

        if (deletedCount == 0)
        {
            logger.LogWarning(
                $"Board with id: {id} does not exist [CorrelationId: {correlationIdAccessor.CorrelationId}]");
            return NotFound($"Board with id {id} not found");
        }

        logger.LogInformation($"Board deleted [{id}] [CorrelationId: {correlationIdAccessor.CorrelationId}");
        return NoContent();
    }
}