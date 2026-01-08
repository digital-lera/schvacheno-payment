Console.WriteLine("Schvacheno Notifications Service v1.0");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging(x => x.AddConsole());

var app = builder.Build();
await app.RunAsync();
