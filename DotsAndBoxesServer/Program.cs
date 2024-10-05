using DotsAndBoxesServerAPI.Models;
using DotsAndBoxesServer.HubFilters;
using DotsAndBoxesServer.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddConsole();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR(hubOptions =>
                                {
                                    hubOptions.EnableDetailedErrors = true;
                                    hubOptions.AddFilter<HubFilter>();
                                });

var app = builder.Build();
app.MapHub<DotsAndBoxesHub>("/dotsAndBoxes");

#region MinimalAPI

app.MapGet("/players/{name}", ([FromRoute] string name) => new Player { Name = "asdasd" });

#endregion

app.Run();