using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/news", () =>
{
	// init with your API key
	Console.WriteLine("API KEY " + app.Configuration["NEWSAPI-API-KEY"].Length);

	var newsApiClient = new NewsApiClient(app.Configuration["NEWSAPI-API-KEY"]);
	var articlesResponse = newsApiClient.GetEverything(new EverythingRequest
	{
		Q = "hundar"
	});
	if (articlesResponse.Status == Statuses.Ok)
	{
		// total results found
		Console.WriteLine(articlesResponse.TotalResults);
		// here's the first 20
		foreach (var article in articlesResponse.Articles)
		{
			// title
			Console.WriteLine(article.Title);
			// author
			Console.WriteLine(article.Author);
			// description
			Console.WriteLine(article.Description);
			// url
			Console.WriteLine(article.Url);
			// published at
			Console.WriteLine(article.PublishedAt);
		}
	} else
	{
		Console.WriteLine("ANALSEX");
	}
});

app.Run();