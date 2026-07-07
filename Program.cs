using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("news", (sp, client) =>
{
	var cfg = sp.GetRequiredService<IConfiguration>();
	client.BaseAddress = new Uri("https://newsapi.org/v2/");
	client.DefaultRequestHeaders.Add("X-Api-Key", cfg["NEWSAPI-API-KEY"]);
	client.DefaultRequestHeaders.UserAgent.ParseAdd("React-NET-Newsreader/1.0");
});

builder.Services.AddSingleton(_ =>
	new AnthropicClient(builder.Configuration["CLAUDE-API-KEY"]));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/news", async (IHttpClientFactory factory, string q) =>
{
	var client = factory.CreateClient("news");
	var resp = await client.GetAsync($"everything?q={Uri.EscapeDataString(q)}");
	var body = await resp.Content.ReadAsStringAsync();
	return Results.Content(body, "application/json");
});

app.MapGet("/llmCatGen", async (AnthropicClient client, String userPrompt, int maxTokenUsage) =>
{
	const int maxTokensPerAPICall = 256;

	if (maxTokenUsage < 0)
	{
		return JsonSerializer.Serialize(new 
		{
			status = "error",
			message = $"maxTokenUsage (current value: {maxTokenUsage}) must be a positive integer."
		});
	}

	if (maxTokenUsage < maxTokensPerAPICall)
	{
		return JsonSerializer.Serialize(new
		{
			status = "error",
			message = $"maxTokenUsage (current value: {maxTokenUsage}) is too low. Set to >= {maxTokensPerAPICall}, please."
		});
	}

	var messages = new List<Message> {
		new Message(RoleType.User, userPrompt)
	};
	
	String pathToPromptFile = Path.Combine(AppContext.BaseDirectory, "newsapi_system_prompt.txt");
	
	var parameters = new MessageParameters
	{
		Messages = messages,
		System = new List<SystemMessage>
		{
			new SystemMessage(getSystemPrompt(pathToPromptFile))
		},
		MaxTokens = maxTokensPerAPICall,
		Model = AnthropicModels.Claude46Sonnet,
		Temperature = 0m,
	};
	
	String raw = (await client.Messages.GetClaudeMessageAsync(parameters)).Message.ToString();
	
	return raw;
});

app.Run();

String getSystemPrompt(String promptFilePath)
{
	String content;

	if (!File.Exists(promptFilePath))
	{
		throw new FileNotFoundException(promptFilePath);
	}

	using (StreamReader reader = new StreamReader(promptFilePath))
	{
		content = reader.ReadToEnd();
	}

	return content;
}