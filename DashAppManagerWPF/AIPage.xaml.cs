using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DashAppManagerWPF
{
    public partial class AIPage : UserControl
    {
        private bool _animationPlayed = false;

        public AIPage()
        {
            InitializeComponent();
            Loaded += AIPage_Loaded;
        }

        private void AIPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Always start the looping pulse animation
            if (this.Resources["GlowPulseAnimation"] is Storyboard pulseStoryboard)
            {
                pulseStoryboard.Begin(this, true);
            }

            if (_animationPlayed) return;
            _animationPlayed = true;

            // Start the one-time entrance animation
            if (this.Resources["EntranceAnimation"] is Storyboard entranceStoryboard)
            {
                entranceStoryboard.Begin(this, true);
            }
        }
    }
}
