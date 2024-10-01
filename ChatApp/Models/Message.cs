namespace ChatApp.Models;

public class Message
{
    public bool IsUser { get; set; }
    public string Role { get; set; }
    public string Content { get; set; }
}
