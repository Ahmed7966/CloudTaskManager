using System.Security.Claims;
using CloudTaskManager;
using CloudTaskManager.Data;
using CloudTaskManager.DTO_s;
using CloudTaskManager.Message;
using CloudTaskManager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
namespace test
{
    public class TaskControllerTests
    {
        [Fact]
        public async Task CreateTask_ShouldPublishEvent_WhenTaskIsCreated()
        {
            var options = new DbContextOptionsBuilder<TaskDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var db = new TaskDbContext(options);

            var mockPublisher = new Mock<IEventPublisher>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-1"),
                new Claim(ClaimTypes.Role, "User")
            }, "TestAuth"));

            var controller = new TaskController(db, mockPublisher.Object);
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var dto = new CreateTaskDto
            {
                Title = "Test Task",
                BoardId = 0,
                ParentTaskId = null
            };

            // Act
            var result = await controller.CreateTask(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdTask = Assert.IsType<TaskResponseDto>(okResult.Value);

            Assert.Equal("Test Task", createdTask.Title);

            // Verify that event publisher was called with same task
            mockPublisher.Verify(p => p.PublishTaskCreated(It.Is<TaskItem>(t => t.Title == "Test Task")),
                Times.Once);
        }
    }

}