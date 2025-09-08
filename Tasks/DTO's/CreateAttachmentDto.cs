namespace CloudTaskManager.DTO_s;

public class CreateAttachmentDto
{
    public string FileUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public Guid TaskItemId { get; set; }
}