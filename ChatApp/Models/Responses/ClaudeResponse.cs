namespace ChatApp.Models.Responses;

public class ClaudeResponse
{
    public string completion { get; set; }  // The generated response from Claude
    public string stop_reason { get; set; }  // The reason the response was stopped (e.g., max tokens, stop sequence, etc.)
}
