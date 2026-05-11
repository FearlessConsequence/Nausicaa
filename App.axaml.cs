using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CourseWork.Models;
using CourseWork.Views;

namespace CourseWork;

public partial class App : Application
{
    public static int CurrentUserId { get; set; } = 0;
    public static UserRole CurrentUserRole { get; set; } = UserRole.PoliceOfficer;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnLastWindowClose;
            desktop.MainWindow = new LoginActitvity();
        }
        base.OnFrameworkInitializationCompleted();
    }
}