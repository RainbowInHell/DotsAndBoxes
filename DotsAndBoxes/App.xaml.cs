using System.Configuration;
using System.Windows;
using AsyncAwaitBestPractices;
using DotsAndBoxesServerAPI;
using Microsoft.Extensions.DependencyInjection;
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
        base.OnStartup(args);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _serviceProvider.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        _serviceProvider.DisposeAsync().SafeFireAndForget(onException: ex => Console.WriteLine(ex.Message));
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var serverAddress = ConfigurationManager.AppSettings["ServerAddress"];
        services.AddRefitClient<IGameAPI>().ConfigureHttpClient(c =>
                                                                    {
                                                                        c.BaseAddress = new Uri(serverAddress!);
                                                                    });
        services.AddViewModels(typeof(BaseViewModel).Assembly);
        services.AddNavigation<BaseViewModel>();

        services.AddSingleton<SignalRClient>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton(serviceProvider => new MainWindow
        {
            DataContext = serviceProvider.GetRequiredService<MainViewModel>()
        });
    }
}