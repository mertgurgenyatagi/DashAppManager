using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using DashAppManagerWPF.Models;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Globalization;
using System.IO;

namespace DashAppManagerWPF
{
    public partial class DashboardView : UserControl
    {
        private DispatcherTimer? _timer;
        private List<Profile>? _profiles;
        private List<Project>? _projects;

        public DashboardView()
        {
            InitializeComponent();
            
            LoadProfiles();
            LoadProjects();
            InitializeTimer();
            UpdateDashboard();
            
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

        private void LoadProfiles()
        {
            try
            {
                _profiles = ProfileDataService.LoadProfiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profiles: {ex.Message}");
                _profiles = new List<Profile>();
            }
        }

        private void LoadProjects()
        {
            try
            {
                _projects = ProjectDataService.LoadProjects();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading projects: {ex.Message}");
                _projects = new List<Project>();
            }
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30) // Update every 30 seconds
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            UpdateDashboard();
        }

        private void UpdateDashboard()
        {
            if (_profiles == null || !_profiles.Any())
            {
                ShowNoProfileDashboard();
                return;
            }

            var currentProfile = GetCurrentProfile();
            if (currentProfile != null)
            {
                UpdateCurrentRoleCard(currentProfile);
                UpdateProjectCards();
                UpdateTaskCounts();
            }
            else
            {
                ShowNoProfileDashboard();
            }
        }

        private Profile? GetCurrentProfile()
        {
            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            foreach (var profile in _profiles!)
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

        private void UpdateCurrentRoleCard(Profile profile)
        {
            // Update role name
            CurrentRoleName.Text = profile.Name;

            // Update time slot
            CurrentRoleTime.Text = $"{profile.StartTime} - {profile.EndTime}";

            // Calculate and display time remaining
            var timeRemaining = CalculateTimeRemaining(profile);
            CurrentRoleRemaining.Text = FormatTimeRemaining(timeRemaining);

            // Update icon
            if (!string.IsNullOrEmpty(profile.Icon))
            {
                try
                {
                    CurrentRoleIcon.Source = new BitmapImage(new Uri($"pack://application:,,,/{profile.Icon}"));
                }
                catch
                {
                    CurrentRoleIcon.Source = null;
                }
            }

            // Extract hex color from icon filename for styling
            var hexColor = ExtractHexFromIcon(profile.Icon);
            var darkerColorBrush = GetDarkerBrushFromHex(hexColor);

            // Update background with darker color
            CurrentRoleCard.Background = darkerColorBrush;
        }

        private void UpdateProjectCards()
        {
            // Get the current project (assume first project is current for now)
            var currentProject = _projects?.FirstOrDefault();
            
            if (currentProject != null)
            {
                CurrentProductTitle.Text = "Current Project";
                CurrentProductName.Text = currentProject.Name;
                CurrentProductDate.Text = $"Started at {currentProject.StartDate:dd MMMM yyyy}";
                
                // Calculate time remaining
                var timeRemaining = currentProject.EndDate - DateTime.Now;
                if (timeRemaining.TotalDays > 1)
                {
                    CurrentProductDuration.Text = $"{(int)timeRemaining.TotalDays} days";
                }
                else if (timeRemaining.TotalHours > 1)
                {
                    CurrentProductDuration.Text = $"{(int)timeRemaining.TotalHours} hours";
                }
                else if (timeRemaining.TotalMinutes > 0)
                {
                    CurrentProductDuration.Text = $"{(int)timeRemaining.TotalMinutes} minutes";
                }
                else
                {
                    CurrentProductDuration.Text = "Project overdue";
                }
                
                CurrentProductStatus.Text = $"Due at {currentProject.EndDate:dd MMMM yyyy}";
            }
            else
            {
                CurrentProductTitle.Text = "Current Project";
                CurrentProductName.Text = "No active project";
                CurrentProductDate.Text = "No project data";
                CurrentProductDuration.Text = "No data";
                CurrentProductStatus.Text = "Configure projects";
            }
        }

        private void UpdateTaskCounts()
        {
            // Load completed tasks to exclude them from count
            var completedTaskIds = LoadCompletedTasks();
            
            // Count unfinished tasks across all profiles
            int unfinishedTasks = 0;
            if (_profiles != null)
            {
                foreach (var profile in _profiles)
                {
                    if (profile.Tasks != null)
                    {
                        foreach (var task in profile.Tasks)
                        {
                            // Generate task ID the same way NotificationsPage does - using profile.Id, not profile.Name
                            var taskId = $"{profile.Id}_{task}";
                            
                            // Only count if not completed
                            if (!completedTaskIds.Contains(taskId))
                            {
                                unfinishedTasks++;
                            }
                        }
                    }
                }
            }

            UnfinishedTasksCount.Text = unfinishedTasks.ToString();

            // Calculate time left at work until 18:00
            var now = DateTime.Now;
            var endOfWork = DateTime.Today.AddHours(18); // 6 PM
            
            if (now.TimeOfDay < TimeSpan.FromHours(18))
            {
                var timeLeft = endOfWork - now;
                TimeLeftAtWorkHours.Text = $"{(int)timeLeft.TotalHours}h {timeLeft.Minutes}m";
            }
            else
            {
                TimeLeftAtWorkHours.Text = "Work day ended";
            }
        }

        private HashSet<string> LoadCompletedTasks()
        {
            var completedTasks = new HashSet<string>();
            try
            {
                var completedTasksFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "completed_tasks.txt");
                if (File.Exists(completedTasksFilePath))
                {
                    var lines = File.ReadAllLines(completedTasksFilePath);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            completedTasks.Add(line.Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading completed tasks: {ex.Message}");
            }
            
            return completedTasks;
        }

        private void ShowNoProfileDashboard()
        {
            CurrentRoleName.Text = "No Active Profile";
            CurrentRoleTime.Text = "Configure profiles";
            CurrentRoleRemaining.Text = "No time data";
            CurrentRoleIcon.Source = null;
            CurrentRoleCard.Background = new SolidColorBrush(Color.FromRgb(30, 35, 41));
            
            UnfinishedTasksCount.Text = "0";
            TimeLeftAtWorkHours.Text = "No data";
        }

        private TimeSpan CalculateTimeRemaining(Profile profile)
        {
            var now = DateTime.Now.TimeOfDay;
            var endTime = ParseTimeString(profile.EndTime);
            var startTime = ParseTimeString(profile.StartTime);

            if (!endTime.HasValue || !startTime.HasValue)
                return TimeSpan.Zero;

            if (endTime.Value > now)
            {
                return endTime.Value - now;
            }
            else if (startTime.Value > endTime.Value) // Overnight profile
            {
                return endTime.Value.Add(TimeSpan.FromDays(1)) - now;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        private string FormatTimeRemaining(TimeSpan timeRemaining)
        {
            if (timeRemaining.TotalMinutes < 1)
            {
                return "Less than 1 minute remaining";
            }
            else if (timeRemaining.TotalHours < 1)
            {
                var minutes = (int)timeRemaining.TotalMinutes;
                return minutes == 1 ? "1 minute remaining" : $"{minutes} minutes remaining";
            }
            else
            {
                var hours = (int)timeRemaining.TotalHours;
                var minutes = timeRemaining.Minutes;
                
                if (hours == 1 && minutes == 0)
                    return "1 hour remaining";
                else if (hours == 1)
                    return $"1 hour and {minutes} minutes remaining";
                else if (minutes == 0)
                    return $"{hours} hours remaining";
                else
                    return $"{hours} hours and {minutes} minutes remaining";
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

        private SolidColorBrush GetDarkerBrushFromHex(string hex)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString($"#{hex}");
                
                // Make color 30% darker
                var darkerColor = Color.FromRgb(
                    (byte)(color.R * 0.7),
                    (byte)(color.G * 0.7),
                    (byte)(color.B * 0.7)
                );
                
                return new SolidColorBrush(darkerColor);
            }
            catch
            {
                return new SolidColorBrush(Color.FromRgb(61, 188, 147)); // Default darker green
            }
        }
    }
}
