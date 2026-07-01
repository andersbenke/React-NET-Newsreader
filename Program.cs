using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient("news", (sp, client) =>
{
	var cfg = sp.GetRequiredService<IConfiguration>();
	client.BaseAddress = new Uri("https://newsapi.org/v2/");
	client.DefaultRequestHeaders.Add("X-Api-Key", cfg["NEWSAPI-API-KEY"]);
	client.DefaultRequestHeaders.UserAgent.ParseAdd("React-NET-Newsreader/1.0");
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/news", async (IHttpClientFactory factory, string q) =>
{
	var client = factory.CreateClient("news");
	var resp = await client.GetAsync($"everything?q={Uri.EscapeDataString(q)}");
	var body = await resp.Content.ReadAsStringAsync();
	return Results.Content(body, "application/json");
});

app.Run();