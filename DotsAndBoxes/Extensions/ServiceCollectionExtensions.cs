using System.Reflection;
using DotsAndBoxes.Navigation;
using DotsAndBoxes.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DotsAndBoxes.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddNavigation<T>(this IServiceCollection services) where T : class, INavigable
    {
        services.AddSingleton<RouteMap<T>>();
        services.AddSingleton<INavigationService<T>, NavigationService<T>>();
    }

    public static void AddViewModels(this IServiceCollection services, Assembly assembly)
    {
        var viewModels = assembly.GetTypes()
                                 .Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(BaseViewModel)));

        foreach (var viewModel in viewModels)
        {
            services.AddTransient(viewModel);
        }
    }
}