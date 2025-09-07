using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Windows.Media;

namespace DashAppManagerWPF
{
    public partial class DashboardPage : UserControl
    {
        private DispatcherTimer? _timeTimer;
        private bool _useHost1 = true; // Toggle between content hosts for transitions
        private ScrollViewer? _currentHost;
        private ScrollViewer? _nextHost;

        public DashboardPage()
        {
            InitializeComponent();
            InitializeTransitionSystem();
            StartTimeTimer();
            
            // Initialize with default task view
            InitializeDefaultView();
            
            // Add drag functionality with HIGHEST PRECEDENCE - using Preview events
            PreviewMouseLeftButtonDown += DashboardPage_PreviewMouseLeftButtonDown;
            MouseLeftButtonDown += DashboardPage_MouseLeftButtonDown;
            
            this.Unloaded += DashboardPage_Unloaded;
        }

        private void InitializeDefaultView()
        {
            // Set the initial content to show tasks by default
            var defaultContent = CreateDefaultContent();
            _currentHost!.Content = defaultContent;
        }

        private void InitializeTransitionSystem()
        {
            _currentHost = RightContentHost1;
            _nextHost = RightContentHost2;
        }

        private void StartTimeTimer()
        {
            _timeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timeTimer.Tick += UpdateTime;
            _timeTimer.Start();
            UpdateTime(null, null); // Initial update
        }

        private void UpdateTime(object? sender, EventArgs? e)
        {
            // Time will be updated through the dynamic content creation
            // No need to update named elements since we're using transition system
        }

        /// <summary>
        /// Show the default task management view
        /// </summary>
        private void ShowDefaultTaskView()
        {
            var defaultContent = CreateDefaultContent();
            TransitionToContent(defaultContent);
        }

        /// <summary>
        /// Universal transition method for right section content changes
        /// This method will be used for ALL content transitions in the right section
        /// </summary>
        private void TransitionToNewContent()
        {
            // This method is now replaced by more specific methods
            // Keeping for backward compatibility
            var defaultContent = CreateDefaultContent();
            TransitionToContent(defaultContent);
        }

        /// <summary>
        /// Creates the default dashboard content (time + profile + tasks)
        /// </summary>
        private StackPanel CreateDefaultContent()
        {
            var stackPanel = new StackPanel { Margin = new Thickness(30) };

            // Always include the time display
            stackPanel.Children.Add(CreateTimeDisplay());

            // Always show profile and tasks in default view
            stackPanel.Children.Add(CreateProfileSection());
            stackPanel.Children.Add(CreateTasksSection());

            return stackPanel;
        }

        private Border CreateTimeDisplay()
        {
            var border = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 35, 41)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 25)
            };

            var stackPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

            var timeText = new TextBlock
            {
                Text = DateTime.Now.ToString("HH:mm"),
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 36,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var dateText = new TextBlock
            {
                Text = DateTime.Now.ToString("dddd, MMMM d"),
                Foreground = System.Windows.Media.Brushes.Gray,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stackPanel.Children.Add(timeText);
            stackPanel.Children.Add(dateText);
            border.Child = stackPanel;

            return border;
        }

        private StackPanel CreateProfileSection()
        {
            var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 25) };
            var grid = new Grid();
            
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var ellipse = new System.Windows.Shapes.Ellipse
            {
                Width = 50,
                Height = 50,
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 99, 71)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(ellipse, 0);

            var textStackPanel = new StackPanel
            {
                Margin = new Thickness(15, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var titleText = new TextBlock
            {
                Text = "Project Manager",
                Foreground = System.Windows.Media.Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 16
            };

            var subtitleText = new TextBlock
            {
                Text = "Active Profile",
                Foreground = System.Windows.Media.Brushes.Gray,
                FontSize = 12
            };

            textStackPanel.Children.Add(titleText);
            textStackPanel.Children.Add(subtitleText);
            Grid.SetColumn(textStackPanel, 1);

            grid.Children.Add(ellipse);
            grid.Children.Add(textStackPanel);
            stackPanel.Children.Add(grid);

            return stackPanel;
        }

        private StackPanel CreateTasksSection()
        {
            var stackPanel = new StackPanel();

            var titleText = new TextBlock
            {
                Text = "Today's Tasks",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel.Children.Add(titleText);

            // Add task items (simplified for demo)
            var tasks = new[]
            {
                ("Review project requirements", "Due: 15:30", "High", System.Windows.Media.Color.FromRgb(50, 205, 50), System.Windows.Media.Color.FromRgb(255, 99, 71)),
                ("Team standup meeting", "Due: 16:00", "Medium", System.Windows.Media.Color.FromRgb(255, 215, 0), System.Windows.Media.Color.FromRgb(255, 215, 0)),
                ("Update project documentation", "Due: 17:30", "Low", System.Windows.Media.Color.FromRgb(70, 130, 180), System.Windows.Media.Color.FromRgb(70, 130, 180)),
                ("Client presentation prep", "Due: Tomorrow 10:00", "High", System.Windows.Media.Color.FromRgb(147, 112, 219), System.Windows.Media.Color.FromRgb(255, 99, 71)),
                ("Code review session", "Due: Tomorrow 14:00", "Medium", System.Windows.Media.Color.FromRgb(50, 205, 50), System.Windows.Media.Color.FromRgb(255, 215, 0))
            };

            foreach (var (title, due, priority, dotColor, priorityColor) in tasks)
            {
                stackPanel.Children.Add(CreateTaskItem(title, due, priority, dotColor, priorityColor));
            }

            return stackPanel;
        }

        private Border CreateTaskItem(string title, string due, string priority, System.Windows.Media.Color dotColor, System.Windows.Media.Color priorityColor)
        {
            var border = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 35, 41)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var ellipse = new System.Windows.Shapes.Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new System.Windows.Media.SolidColorBrush(dotColor),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(ellipse, 0);

            var textStackPanel = new StackPanel { Margin = new Thickness(12, 0, 0, 0) };

            var titleText = new TextBlock
            {
                Text = title,
                Foreground = System.Windows.Media.Brushes.White,
                FontWeight = FontWeights.Medium
            };

            var dueText = new TextBlock
            {
                Text = due,
                Foreground = System.Windows.Media.Brushes.Gray,
                FontSize = 11
            };

            textStackPanel.Children.Add(titleText);
            textStackPanel.Children.Add(dueText);
            Grid.SetColumn(textStackPanel, 1);

            var priorityText = new TextBlock
            {
                Text = priority,
                Foreground = new System.Windows.Media.SolidColorBrush(priorityColor),
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(priorityText, 2);

            grid.Children.Add(ellipse);
            grid.Children.Add(textStackPanel);
            grid.Children.Add(priorityText);
            border.Child = grid;

            return border;
        }

        /// <summary>
        /// Public method to transition to any new content in the right section
        /// This will be used by other components to trigger transitions
        /// </summary>
        public void TransitionToContent(FrameworkElement newContent)
        {
            // Disable interactions during transition
            RightSectionTransitionContainer.IsHitTestVisible = false;

            // Set up the transition hosts
            _nextHost!.Content = newContent;
            _nextHost.Opacity = 0;
            _nextHost.IsHitTestVisible = false;

            // Create smooth fade transition animations
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));

            // Add easing for smoother transitions
            var easing = new QuadraticEase { EasingMode = EasingMode.EaseInOut };
            fadeOut.EasingFunction = easing;
            fadeIn.EasingFunction = easing;

            // Start fade out animation on current host
            _currentHost!.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            // Start fade in animation on next host
            var fadeInDelay = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            {
                BeginTime = TimeSpan.FromMilliseconds(50),
                EasingFunction = easing
            };
            _nextHost.BeginAnimation(UIElement.OpacityProperty, fadeInDelay);

            // Handle transition completion
            fadeOut.Completed += (s, e) =>
            {
                // Clear old content and reset old host
                _currentHost.Content = null;
                _currentHost.Opacity = 1;
                _currentHost.IsHitTestVisible = true;

                // Enable interactions again
                RightSectionTransitionContainer.IsHitTestVisible = true;
                _nextHost.IsHitTestVisible = true;

                // Swap the hosts for next transition
                (_currentHost, _nextHost) = (_nextHost, _currentHost);
                _useHost1 = !_useHost1;
            };
        }

        private void DashboardPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _timeTimer?.Stop();
        }

        /// <summary>
        /// ABSOLUTE HIGHEST PRECEDENCE drag functionality - Direct overlay element
        /// This will fire before ANY other control because it's on top with highest Z-Index
        /// </summary>
        private void DragZoneOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window? parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                // This overlay only exists in the top 70px, so always allow drag
                e.Handled = true; // STOP event propagation immediately
                parentWindow.DragMove();
            }
        }

        /// <summary>
        /// HIGHEST PRECEDENCE drag functionality - Preview event fires BEFORE all other events
        /// This ensures drag takes precedence over ANY child control interactions
        /// </summary>
        private void DashboardPage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window? parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                // Only allow drag if mouse is within top 70px of window
                var position = e.GetPosition(parentWindow);
                if (position.Y <= 70)
                {
                    // CRITICAL: Mark event as handled to prevent it from reaching child controls
                    e.Handled = true;
                    parentWindow.DragMove();
                    return; // Exit immediately after starting drag
                }
            }
        }

        /// <summary>
        /// Backup drag functionality - in case Preview event doesn't fire
        /// </summary>
        private void DashboardPage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window? parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                var position = e.GetPosition(parentWindow);
                if (position.Y <= 70)
                {
                    e.Handled = true;
                    parentWindow.DragMove();
                }
            }
        }
    }
}
