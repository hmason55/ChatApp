namespace ChatApp.Models.Responses;

public class OpenAIResponse
{
    public List<Choice> choices { get; set; }  // List of possible responses

    public class Choice
    {
        public Message message { get; set; }  // Contains the actual message content
    }

    public class Message
    {
        public string role { get; set; }  // The role of the sender (e.g., "assistant")
        public string content { get; set; }  // The actual response content
    }
}
