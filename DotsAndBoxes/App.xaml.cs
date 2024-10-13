using System.Configuration;
using System.Windows;
using AsyncAwaitBestPractices;
using DotsAndBoxes.Extensions;
using DotsAndBoxes.SignalR;
using DotsAndBoxes.ViewModels;
using DotsAndBoxes.Views;
using DotsAndBoxesServerAPI.Refit;
using Microsoft.Extensions.DependencyInjection;
using NReco.Logging.File;
using Refit;

namespace DotsAndBoxes;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private ServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs args)
    {
        // AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                                                          // {
                                                              // _ = new CustomMessageBox("Необработанное исключение.", MessageType.Error, MessageButtons.Ok).ShowDialog();
                                                              // Application.OnThreadException(exception);
                                                              // TODO: Logging
                                                          // };

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _serviceProvider.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Task.Run(async () =>
                     {
                         // This will dispose all registered IDisposables.
                         await _serviceProvider.DisposeAsync().ConfigureAwait(false);
                     }).SafeFireAndForget(onException: ex => Console.WriteLine(ex.Message));

        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // services.AddLogging(loggingBuilder =>
        //                         {
        //                             loggingBuilder.AddFile("Logs/DotsAndBoxesUI_{0:yyyy}-{0:MM}-{0:dd}.log",
        //                                                    fileLoggerOpts =>
        //                                                        {
        //                                                            fileLoggerOpts.FormatLogFileName = fName => string.Format(fName, DateTime.UtcNow);
        //                                                        });
        //                         });

        var serverAddress = ConfigurationManager.AppSettings["ServerAddress"];
        services.AddRefitClient<IGameAPI>().ConfigureHttpClient(c =>
                                                                    {
                                                                        c.BaseAddress = new Uri(serverAddress!);
                                                                    });
        services.AddViewModels(typeof(BaseViewModel).Assembly);
        services.AddNavigation<BaseViewModel>();

        services.AddSingleton<SignalRServer>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton(typeof(MainWindow));
    }
}