using System.Windows;
using DotsAndBoxes.Extensions;
using DotsAndBoxes.ViewModels;
using DotsAndBoxes.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotsAndBoxes;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs args)
    {
        var configuration = new ConfigurationBuilder().Build();
        var serviceCollection = new ServiceCollection();

        ConfigureServices(serviceCollection, configuration);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNavigation<BaseViewModel>();
        services.RegisterTransientViewModelsFromAssembly(typeof(BaseViewModel).Assembly);

        services.AddSingleton<MainViewModel>();
        services.AddSingleton(typeof(MainWindow));

        services.AddSingleton(configuration);
    }
}