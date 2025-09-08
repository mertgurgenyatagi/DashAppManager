using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Interop;

namespace SPMWPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Enable hardware acceleration if available
        if (RenderCapability.Tier > 0)
        {
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 60 }
            );
        }
        
        base.OnStartup(e);

        // Create the main window but don't show it yet. This ensures there is an application
        // window even if the welcome dialog encounters problems.
    MainWindow? main = null;
        try
        {
            main = new MainWindow();
            this.MainWindow = main;

            // Show the main window first so Owner can be set for the welcome dialog
            main.Show();

            if (!SettingsStore.GetDontShowWelcome())
            {
                var welcome = new WelcomeWindow();
                // Ensure the welcome dialog appears on top of the main window
                welcome.Owner = this.MainWindow;
                welcome.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                welcome.ShowDialog();
            }
        }
        catch (System.Exception ex)
        {
            // If anything fails here, write to console so the run terminal shows the error.
            System.Console.Error.WriteLine($"Startup error: {ex}");
            // Attempt to show the main window if it exists
            try { main?.Show(); } catch { }
        }
    }
}

