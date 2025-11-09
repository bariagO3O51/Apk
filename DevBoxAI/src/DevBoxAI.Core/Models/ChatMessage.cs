namespace DevBoxAI.Core.Models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public ChatRole Role { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public List<Attachment>? Attachments { get; set; }
}

public enum ChatRole
{
    User,
    Assistant,
    System
}

public class Attachment
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public AttachmentType Type { get; set; }
}

public enum AttachmentType
{
    Image,
    Document,
    Code
}
