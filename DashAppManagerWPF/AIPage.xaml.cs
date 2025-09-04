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
            if (!_animationPlayed)
            {
                _animationPlayed = true;

                if (this.Resources["EntranceAnimation"] is Storyboard entranceStoryboard)
                {
                    // When the entrance animation is complete, start the looping pulse
                    entranceStoryboard.Completed += (s, ev) => {
                        if (this.Resources["GlowPulseAnimation"] is Storyboard pulseStoryboard)
                        {
                            pulseStoryboard.Begin(this, true);
                        }
                    };
                    entranceStoryboard.Begin(this, true);
                }
            }
            else
            {
                // If we are returning to the page, just make sure the pulse is running.
                if (this.Resources["GlowPulseAnimation"] is Storyboard pulseStoryboard)
                {
                    pulseStoryboard.Begin(this, true);
                }
            }
        }
    }
}
