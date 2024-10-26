using DotsAndBoxesServer;
using DotsAndBoxesServer.HubFilters;
using DotsAndBoxesServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using NReco.Logging.File;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging(loggingBuilder =>
                                {
                                    loggingBuilder.AddFile("Logs/DotsAndBoxesServer_{0:yyyy}-{0:MM}-{0:dd}.log",
                                                           fileLoggerOpts =>
                                                               {
                                                                   fileLoggerOpts.FormatLogFileName = fName => string.Format(fName, DateTime.UtcNow);
                                                               });
                                    loggingBuilder.AddConsole();
                                });
builder.Services.AddSingleton<PlayersManager>();
builder.Services.AddSignalR(hubOptions =>
                                {
                                    hubOptions.EnableDetailedErrors = true;
                                    hubOptions.AddFilter<HubFilter>();
                                });

var app = builder.Build();
app.MapHub<DotsAndBoxesHub>("/dotsAndBoxes");

#region MinimalAPI

app.MapGet("/players", (PlayersManager playersManager) => playersManager.GetConnectedPlayers());

#endregion

app.Run();