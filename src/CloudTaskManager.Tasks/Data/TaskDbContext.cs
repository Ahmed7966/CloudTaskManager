using CloudTaskManager.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudTaskManager.Data;

public class TaskDbContext(DbContextOptions<TaskDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Reminder> Reminders => Set<Reminder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 🔹 TaskItem
        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.AssignedToUserId);

        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.ParentTask)
            .WithMany(t => t.SubTasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 Board
        modelBuilder.Entity<Board>()
            .HasMany(b => b.Tasks)
            .WithOne(t => t.Board)
            .HasForeignKey(t => t.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Board>()
            .HasMany(b => b.Members)
            .WithOne(m => m.Board)
            .HasForeignKey(m => m.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 Comment
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.TaskItem)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 Label
        modelBuilder.Entity<Label>()
            .HasOne(l => l.TaskItem)
            .WithMany(t => t.Labels)
            .HasForeignKey(l => l.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 Attachment
        modelBuilder.Entity<Attachment>()
            .HasOne(a => a.TaskItem)
            .WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 Reminder
        modelBuilder.Entity<Reminder>()
            .HasOne(r => r.TaskItem)
            .WithMany(t => t.Reminders)
            .HasForeignKey(r => r.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 Seed Data
        var boardId = 1;
        var task1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var task2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<Board>().HasData(new Board
        {
            Id = boardId,
            Name = "Demo Project",
            Description = "This is a demo board for testing."
        });

        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem
            {
                Id = task1Id,
                Title = "Setup project",
                Description = "Initialize solution, setup GitHub repo",
                Status = Status.Pending,
                DueDate = new DateTime(2025, 9, 5),
                BoardId = boardId,
                CreatedAt = new DateTime(2025, 9, 1),
                UpdatedAt = new DateTime(2025, 9, 1)
            },
            new TaskItem
            {
                Id = task2Id,
                Title = "Implement login",
                Description = "Add Identity + JWT auth",
                Status = Status.InProgress,
                DueDate = new DateTime(2025, 9, 10),
                BoardId = boardId,
                CreatedAt = new DateTime(2025, 9, 1),
                UpdatedAt = new DateTime(2025, 9, 1)
            }
        );

        modelBuilder.Entity<Member>().HasData(
            new Member { Id = 1, BoardId = boardId, UserId = "demo-user-1", Role = "BoardOwner" },
            new Member { Id = 2, BoardId = boardId, UserId = "demo-user-2", Role = "User" }
        );
        
        modelBuilder.Entity<Comment>().HasData(
            new Comment
            {
                Id = 1,
                Content = "This is the first comment on Setup project",
                TaskItemId = task1Id,
                CreatedAt = new DateTime(2025, 9, 2),
                UserId = "demo-user-1"
            },
            new Comment
            {
                Id = 2,
                Content = "Make sure to use JWT Bearer tokens",
                TaskItemId = task2Id,
                CreatedAt = new DateTime(2025, 9, 3),
                UserId = "demo-user-2"
            }
        );
        modelBuilder.Entity<Label>().HasData(
            new Label { Id = 1, Name = "Backend", Color = "#FF5733", TaskItemId = task1Id },
            new Label { Id = 2, Name = "Auth", Color = "#33C1FF", TaskItemId = task2Id }
        );
        modelBuilder.Entity<Reminder>().HasData(
            new Reminder
            {
                Id = 1,
                TaskItemId = task1Id,
                ReminderTime = new DateTime(2025, 9, 4),
            },
            new Reminder
            {
                Id = 2,
                TaskItemId = task2Id,
                ReminderTime = new DateTime(2025, 9, 9),
            }
        );
        modelBuilder.Entity<Attachment>().HasData(
            new Attachment
            {
                Id = 1,
                TaskItemId = task1Id,
                FileUrl = "/files/project-setup-guide.pdf",
                FileName = "ProjectSetupGuide.pdf",
                UploadedAt = new DateTime(2025, 9, 2)
            },
            new Attachment
            {
                Id = 2,
                TaskItemId = task2Id,
                FileUrl = "/files/auth-diagram.png",
                FileName = "AuthDiagram.png",
                UploadedAt = new DateTime(2025, 9, 3)
            }
        );

    }
}
