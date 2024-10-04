using System.Reflection;
using DotsAndBoxes.Attributes;
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

    public static void RegisterTransientViewModelsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var viewModels = assembly
            .GetTypes()
            .Where(x => !x.IsAbstract
                        && x.IsSubclassOf(typeof(BaseViewModel))
                        && x.IsDefined(typeof(ViewModelLifetimeAttribute), true)
                        && GetViewModelLifetimeFromAttribute(x) == ServiceLifetime.Transient);

        foreach (var viewModel in viewModels)
        {
            services.AddTransient(viewModel);
        }
    }
    
    private static ServiceLifetime GetViewModelLifetimeFromAttribute(Type t)
    {
        var attribute = Attribute.GetCustomAttribute(t, typeof(ViewModelLifetimeAttribute))!;
        return ((ViewModelLifetimeAttribute)attribute).Lifetime;
    }
}