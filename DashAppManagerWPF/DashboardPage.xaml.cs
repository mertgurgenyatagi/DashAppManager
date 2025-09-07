using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Windows.Media;
using DashAppManagerWPF.Models;
using System.Windows.Media.Imaging;

namespace DashAppManagerWPF
{
    public partial class DashboardPage : UserControl
    {
        private DispatcherTimer? _timeTimer;
        private bool _useHost1 = true; // Toggle between content hosts for transitions
        private ScrollViewer? _currentHost;
        private ScrollViewer? _nextHost;
        private List<Profile>? _profiles;

        public DashboardPage()
        {
            InitializeComponent();
            LoadProfiles();
            InitializeTransitionSystem();
            StartTimeTimer();
            
            // Initialize with default task view
            InitializeDefaultView();
            
            // Add drag functionality with HIGHEST PRECEDENCE - using Preview events
            PreviewMouseLeftButtonDown += DashboardPage_PreviewMouseLeftButtonDown;
            MouseLeftButtonDown += DashboardPage_MouseLeftButtonDown;
            
            this.Unloaded += DashboardPage_Unloaded;
        }

        private void LoadProfiles()
        {
            try
            {
                _profiles = ProfileDataService.LoadProfiles();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profiles: {ex.Message}");
                _profiles = new List<Profile>();
            }
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

            // Get current profile
            var currentProfile = GetCurrentProfile();
            
            if (currentProfile != null)
            {
                // Create icon border instead of simple ellipse
                var iconBorder = new Border
                {
                    Width = 50,
                    Height = 50,
                    CornerRadius = new CornerRadius(25),
                    Background = GetBrushFromHex(ExtractHexFromIcon(currentProfile.Icon)),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                // Add icon if available
                if (!string.IsNullOrEmpty(currentProfile.Icon))
                {
                    var image = new Image
                    {
                        Width = 35,
                        Height = 35
                    };
                    
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                    
                    try
                    {
                        image.Source = new BitmapImage(new Uri($"pack://application:,,,/{currentProfile.Icon}"));
                    }
                    catch
                    {
                        // Fallback if icon can't be loaded
                    }
                    
                    iconBorder.Child = image;
                }
                
                Grid.SetColumn(iconBorder, 0);

                var textStackPanel = new StackPanel
                {
                    Margin = new Thickness(15, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var titleText = new TextBlock
                {
                    Text = currentProfile.Name,
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

                grid.Children.Add(iconBorder);
                grid.Children.Add(textStackPanel);
            }
            else
            {
                // No active profile
                var ellipse = new System.Windows.Shapes.Ellipse
                {
                    Width = 50,
                    Height = 50,
                    Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 60, 60)),
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
                    Text = "No Active Profile",
                    Foreground = System.Windows.Media.Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16
                };

                var subtitleText = new TextBlock
                {
                    Text = "Configure profiles",
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontSize = 12
                };

                textStackPanel.Children.Add(titleText);
                textStackPanel.Children.Add(subtitleText);
                Grid.SetColumn(textStackPanel, 1);

                grid.Children.Add(ellipse);
                grid.Children.Add(textStackPanel);
            }
            
            stackPanel.Children.Add(grid);
            return stackPanel;
        }

        private StackPanel CreateTasksSection()
        {
            var stackPanel = new StackPanel();

            var titleText = new TextBlock
            {
                Text = "Tasks",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel.Children.Add(titleText);

            // Get current profile tasks
            var currentProfile = GetCurrentProfile();
            if (currentProfile != null && currentProfile.Tasks != null && currentProfile.Tasks.Any())
            {
                // Get the profile's icon color for all task dots
                var hexColor = ExtractHexFromIcon(currentProfile.Icon);
                var profileColor = GetColorFromHex(hexColor);

                for (int i = 0; i < currentProfile.Tasks.Count; i++)
                {
                    var task = currentProfile.Tasks[i];
                    stackPanel.Children.Add(CreateTaskItem(task, profileColor));
                }
            }
            else
            {
                // Show "no tasks" message
                var noTasksText = new TextBlock
                {
                    Text = "No tasks for current profile",
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                stackPanel.Children.Add(noTasksText);
            }

            return stackPanel;
        }

        private Border CreateTaskItem(string title, System.Windows.Media.Color dotColor)
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

            textStackPanel.Children.Add(titleText);
            Grid.SetColumn(textStackPanel, 1);

            grid.Children.Add(ellipse);
            grid.Children.Add(textStackPanel);
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

        private Profile? GetCurrentProfile()
        {
            if (_profiles == null || !_profiles.Any())
                return null;

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            foreach (var profile in _profiles)
            {
                var startTime = ParseTimeString(profile.StartTime);
                var endTime = ParseTimeString(profile.EndTime);
                
                if (startTime.HasValue && endTime.HasValue)
                {
                    if (IsTimeInRange(currentTime, startTime.Value, endTime.Value))
                    {
                        return profile;
                    }
                }
            }

            return null;
        }

        private TimeSpan? ParseTimeString(string timeString)
        {
            if (string.IsNullOrEmpty(timeString))
                return null;

            if (TimeSpan.TryParse(timeString, out var result))
                return result;

            return null;
        }

        private bool IsTimeInRange(TimeSpan currentTime, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime <= endTime)
            {
                return currentTime >= startTime && currentTime < endTime;
            }
            else
            {
                return currentTime >= startTime || currentTime < endTime;
            }
        }

        private string ExtractHexFromIcon(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath))
                return "3dbc93"; // Default color

            var filename = System.IO.Path.GetFileNameWithoutExtension(iconPath);
            
            if (filename != null && filename.Length == 6 && IsValidHex(filename))
            {
                return filename;
            }

            return "3dbc93"; // Default fallback
        }

        private bool IsValidHex(string hex)
        {
            return hex.All(c => "0123456789ABCDEFabcdef".Contains(c));
        }

        private System.Windows.Media.SolidColorBrush GetBrushFromHex(string hex)
        {
            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString($"#{hex}");
                return new System.Windows.Media.SolidColorBrush(color);
            }
            catch
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 188, 147)); // Default green
            }
        }

        private System.Windows.Media.Color GetColorFromHex(string hex)
        {
            try
            {
                return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString($"#{hex}");
            }
            catch
            {
                return System.Windows.Media.Color.FromRgb(61, 188, 147); // Default green
            }
        }
    }
}
