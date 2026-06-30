var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/newscategoriesfromprompt", () =>
{
	return "En stor hest";
});

app.Run();