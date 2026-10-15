using System;
using Microsoft.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using uPowerMonitoring.ViewModels;
using uPowerMonitoring.Views;
using uPowerMonitoring.Services;

using LiveChartsCore; // WICHTIG
using LiveChartsCore.SkiaSharpView; // WICHTIG

namespace uPowerMonitoring;

public partial class App : Application
{
    // Diese Eigenschaft macht den Container zentral verfügbar (falls man ihn mal manuell braucht)
    public static IServiceProvider? Services { get; private set; }
    public override void Initialize()
    {
       try
        {
            var services = new ServiceCollection();
            services.AddSingleton<IIna228Service>(provider => new Ina228I2cService());
            services.AddSingleton<ISampleService, SampleService>();
            services.AddSingleton<DashBoardViewModel>();
            services.AddTransient<MainViewModel>();
           

            Services = services.BuildServiceProvider();

            AvaloniaXamlLoader.Load(this);
        }
        catch (Exception ex)
        {
            // Setze hier einen Breakpoint oder gib ex.InnerException aus!
            System.Diagnostics.Debug.WriteLine($"FEHLER BEIM START: {ex}");
            throw;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        LiveCharts.Configure(config =>
            config
                .AddSkiaSharp()
                .AddDefaultMappers()
        );
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services?.GetRequiredService<MainViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}