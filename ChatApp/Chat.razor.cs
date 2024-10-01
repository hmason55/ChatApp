using ChatApp.Models;
using ChatApp.Models.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ChatApp;

public partial class Chat : ComponentBase
{
    private List<Message> messages = [];
    private string userInput = string.Empty;

    private string selectedProvider = "OpenAI";  // Default provider
    private string previousProvider;

    private string selectedModel = "gpt-3.5-turbo";  // Default OpenAI model

    private Dictionary<string, string> apiKeys;
    private Dictionary<string, string> apiEndpoints;

    // Initialize API keys and endpoints
    protected override void OnInitialized()
    {
        apiKeys = new Dictionary<string, string>
        {
            { "OpenAI", Configuration["OpenAI:ApiKey"] },
            { "Claude", Configuration["Claude:ApiKey"] },
            { "LMStudio", Configuration["LMStudio:ApiKey"] }
        };

        apiEndpoints = new Dictionary<string, string>
        {
            { "OpenAI", Configuration["OpenAI:Endpoint"] },
            { "Claude", Configuration["Claude:Endpoint"] },
            { "LMStudio", Configuration["LMStudio:Endpoint"] }
        };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (previousProvider != selectedProvider)
        {
            previousProvider = selectedProvider;
            selectedModel = Configuration.GetSection($"{selectedProvider}:Models")?.Get<string[]>()?.FirstOrDefault() ?? "None";
            StateHasChanged();
        }

        await JS.InvokeVoidAsync("highlightCode");
    }

    private async Task SendMessage()
    {
        // Validate input
        if (string.IsNullOrEmpty(userInput)) return;

        // Fetch API key and endpoint for the selected provider
        var apiKey = apiKeys[selectedProvider];
        var endpoint = apiEndpoints[selectedProvider];

        try
        {
            // Send the message and get a reply
            var botReply = await SendMessageAsync(userInput, apiKey, selectedProvider, selectedModel, endpoint);

            // Add the user message and bot reply to the message list
            messages.Add(new Message { Role = "User", Content = userInput });
            messages.Add(new Message { Role = "Bot", Content = botReply });

            // Clear the user input after message is sent
            userInput = string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    private async Task<string> SendMessageAsync(string userInput, string apiKey, string provider, string model, string endpoint)
    {
        // Switch provider and call respective API
        return provider switch
        {
            "OpenAI" => await SendMessageToOpenAIAsync(userInput, apiKey, model, endpoint),
            "Claude" => await SendMessageToClaudeAsync(userInput, apiKey, model, endpoint),
            "LMStudio" => await SendMessageToLMStudioAsync(userInput, endpoint),
            _ => throw new ArgumentException("Unknown provider specified"),
        };
    }

    // Sends messages to OpenAI or Claude API
    private async Task<string> SendMessageToOpenAIAsync(string userInput, string apiKey, string model, string endpoint)
    {
        var request = new
        {
            model,
            messages = new[] { new Message { Role = "user", Content = userInput } }
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(request)
        };

        if (!string.IsNullOrEmpty(apiKey))
        {
            httpRequestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");
        }

        try
        {
            var response = await Http.SendAsync(httpRequestMessage);
            var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
            return result?.choices?[0]?.message?.content ?? "No response received.";
        }
        catch (Exception e)
        {
            return $"An error occurred while sending the request: {e.Message}";
        }

    }

    private async Task<string> SendMessageToClaudeAsync(string userInput, string apiKey, string model, string endpoint)
    {
        // Request structure for Claude's API
        var request = new
        {
            prompt = userInput,  // The prompt to send to Claude
            model,  // Model selection, e.g., "claude-v1" or "claude-v2"
            max_tokens_to_sample = 1000,  // Maximum tokens to generate
            stop_sequences = new string[] { "\n" },  // Optional stop sequences
            temperature = 0.7  // Optional temperature setting for response diversity
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(request)
        };

        // Add the API key to the request header
        if (!string.IsNullOrEmpty(apiKey))
        {
            httpRequestMessage.Headers.Add("x-api-key", apiKey);  // Add the API key header
            httpRequestMessage.Headers.Add("anthropic-version", "2023-06-01");  // Specify the Anthropic version

        }

        httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        // TODO fix credits too low on anthropic

        try
        {
            // Send the request
            var response = await Http.SendAsync(httpRequestMessage);

            // Read and deserialize the response
            var result = await response.Content.ReadFromJsonAsync<ClaudeResponse>();

            // Return the generated completion (response)
            return result?.completion ?? "No response received.";
        }
        catch (Exception e)
        {
            // Handle any errors during the request
            return $"An error occurred while sending the request: {e.Message}";
        }
    }

    // Sends messages to LMStudio (local model)
    private async Task<string> SendMessageToLMStudioAsync(string userInput, string endpoint)
    {
        var request = new
        {
            prompt = userInput,
            max_tokens = 1000
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(request)
        };

        var response = await Http.SendAsync(httpRequestMessage);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LMStudioResponse>();
        return result?.result ?? "No response received.";
    }
}
