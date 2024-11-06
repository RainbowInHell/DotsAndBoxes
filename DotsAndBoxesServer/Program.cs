using DotsAndBoxesServer;
using Microsoft.Extensions.Logging.Console;
using NReco.Logging.File;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging(loggingBuilder =>
                                {
                                    loggingBuilder.AddFile("Logs/DotsAndBoxesServer_{0:yyyy}-{0:MM}-{0:dd}.log",
                                                           fileLoggerOpts =>
                                                               {
                                                                   fileLoggerOpts.FormatLogFileName = fName => string.Format(fName, DateTime.UtcNow);
                                                                   fileLoggerOpts.FormatLogEntry = msg =>
                                                                                                       {
                                                                                                           var logTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                                                                           return $"{logTime} {msg.LogLevel} {msg.Message}{Environment.NewLine}";
                                                                                                       };
                                                               })
                                        .AddSimpleConsole(options =>
                                                              {
                                                                  options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                                                                  options.ColorBehavior = LoggerColorBehavior.Enabled;
                                                                  options.IncludeScopes = true;
                                                                  options.SingleLine = true;
                                                              });
                                });


builder.Services.AddSingleton<GameManager>();
builder.Services.AddSignalR(hubOptions =>
                                {
                                    hubOptions.EnableDetailedErrors = true;
                                });

var app = builder.Build();
app.MapHub<DotsAndBoxesHub>("/dotsAndBoxes");

#region MinimalAPI

app.MapGet("/players", (GameManager playersManager) => playersManager.GetConnectedPlayers());

#endregion

app.Run();