using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonHard.AmvrosiaTaxSync.Application.Contracts;
using MoonHard.AmvrosiaTaxSync.Application.Services;
using MoonHard.AmvrosiaTaxSync.Desktop.Logging;
using MoonHard.AmvrosiaTaxSync.Desktop.Services;
using MoonHard.AmvrosiaTaxSync.Desktop.ViewModels;
using MoonHard.AmvrosiaTaxSync.Infrastructure.SqlServer.SqlServer;

namespace MoonHard.AmvrosiaTaxSync.Desktop;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var registrations = new ServiceCollection();
        registrations.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddProvider(new JsonFileLoggerProvider());
        });
        registrations.AddSingleton<ISqlSchemaService, SqlSchemaService>();
        registrations.AddSingleton<ConnectionPreferencesStore>();
        registrations.AddSingleton<UdlConnectionFileReader>();
        registrations.AddSingleton<ThemeService>();
        registrations.AddTransient<MainViewModel>();
        registrations.AddTransient<MainWindow>();

        _services = registrations.BuildServiceProvider();
        _services.GetRequiredService<ILogger<App>>().LogInformation("Application started.");
        var window = _services.GetRequiredService<MainWindow>();
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.GetService<ILogger<App>>()?.LogInformation("Application exited.");
        _services?.Dispose();
        base.OnExit(e);
    }
}
