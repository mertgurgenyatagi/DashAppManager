using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace DashAppManagerWPF
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            CurrentRoleCard.MouseLeftButtonDown += CurrentRoleCard_MouseLeftButtonDown;
            
            // Add click handlers for stat cards
            CurrentProductCard.MouseLeftButtonDown += CurrentProductCard_MouseLeftButtonDown;
            TimeRemainingCard.MouseLeftButtonDown += TimeRemainingCard_MouseLeftButtonDown;
            UnfinishedTasksCard.MouseLeftButtonDown += UnfinishedTasksCard_MouseLeftButtonDown;
            TimeLeftAtWorkCard.MouseLeftButtonDown += TimeLeftAtWorkCard_MouseLeftButtonDown;
        }

        private void CurrentRoleCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Navigate to profile page by triggering the main window's navigation system
            NavigateToProfilePage();
        }

        private void CurrentProductCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Navigate to calendar page
            NavigateToPage("calendar");
        }

        private void TimeRemainingCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Navigate to calendar page
            NavigateToPage("calendar");
        }

        private void UnfinishedTasksCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Navigate to notifications page
            NavigateToPage("notifications");
        }

        private void TimeLeftAtWorkCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Navigate to configuration page
            NavigateToPage("config");
        }

        private void NavigateToProfilePage()
        {
            // Navigate to profile page
            NavigateToPage("user");
        }

        private void NavigateToPage(string pageName)
        {
            // Find the MainWindow and call its ShowPage method
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                // Use reflection to call the private ShowPage method
                var showPageMethod = typeof(MainWindow).GetMethod("ShowPage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                showPageMethod?.Invoke(mainWindow, new object[] { pageName });
                
                // Also update the navigation button state
                UpdateNavigationButtonState(mainWindow, pageName);
            }
        }

        private void UpdateNavigationButtonState(MainWindow mainWindow, string pageName)
        {
            // Use reflection to access and update the navigation button state
            var updateLastClickedButtonMethod = typeof(MainWindow).GetMethod("UpdateLastClickedButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Map page names to button positions
            var buttonPositions = new Dictionary<string, double>
            {
                { "dashboard", 188 },
                { "user", 237 },
                { "notifications", 286 },
                { "calendar", 335 },
                { "config", 384 },
                { "ai", 433 },
                { "settings", 482 }
            };

            if (buttonPositions.TryGetValue(pageName, out double targetPosition))
            {
                // Find the appropriate button
                var navCanvas = mainWindow.FindName("NavCanvas") as System.Windows.Controls.Canvas;
                if (navCanvas != null)
                {
                    foreach (var child in navCanvas.Children)
                    {
                        if (child is System.Windows.Controls.Button btn)
                        {
                            var top = System.Windows.Controls.Canvas.GetTop(btn);
                            if (Math.Abs(top - targetPosition) < 1) // Use small tolerance for double comparison
                            {
                                updateLastClickedButtonMethod?.Invoke(mainWindow, new object[] { btn });
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
