using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Interop;

namespace DashAppManagerWPF;

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
    }
}

