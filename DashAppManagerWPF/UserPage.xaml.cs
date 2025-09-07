using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using DashAppManagerWPF.Models;

namespace DashAppManagerWPF
{
    public partial class UserPage : UserControl
    {
        private DispatcherTimer? _timer;
        private List<Profile>? _profiles;

        public UserPage()
        {
            InitializeComponent();
            LoadProfiles();
            SetupTimer();
            UpdateCurrentProfile();
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

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // Update every second
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            UpdateCurrentProfile();
        }

        private void UpdateCurrentProfile()
        {
            if (_profiles == null || !_profiles.Any())
            {
                ShowNoProfileMessage();
                return;
            }

            // Debug: Show current time and profile count
            var now = DateTime.Now;
            System.Diagnostics.Debug.WriteLine($"Current time: {now:HH:mm:ss} ({now.TimeOfDay})");
            System.Diagnostics.Debug.WriteLine($"Loaded {_profiles.Count} profiles");

            var currentProfile = GetCurrentProfile();
            if (currentProfile != null)
            {
                System.Diagnostics.Debug.WriteLine($"Found active profile: {currentProfile.Name}");
                DisplayProfile(currentProfile);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No active profile found");
                ShowNoProfileMessage();
            }
        }

        private Profile? GetCurrentProfile()
        {
            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            System.Diagnostics.Debug.WriteLine($"Looking for profile at time: {currentTime:hh\\:mm}");

            foreach (var profile in _profiles!)
            {
                var startTime = ParseTimeString(profile.StartTime);
                var endTime = ParseTimeString(profile.EndTime);
                
                System.Diagnostics.Debug.WriteLine($"Checking profile '{profile.Name}': {profile.StartTime} - {profile.EndTime}");
                
                if (startTime.HasValue && endTime.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"  Parsed: {startTime.Value:hh\\:mm} - {endTime.Value:hh\\:mm}");
                    
                    if (IsTimeInRange(currentTime, startTime.Value, endTime.Value))
                    {
                        System.Diagnostics.Debug.WriteLine($"  MATCH! Current time {currentTime:hh\\:mm} is in range");
                        return profile;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  No match for current time {currentTime:hh\\:mm}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  Failed to parse times");
                }
            }

            return null;
        }

        private TimeSpan? ParseTimeString(string timeString)
        {
            if (string.IsNullOrEmpty(timeString))
                return null;

            // Parse 24-hour format (e.g., "21:00", "09:30")
            if (TimeSpan.TryParse(timeString, out var result))
                return result;

            return null;
        }

        private bool IsTimeInRange(TimeSpan currentTime, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime <= endTime)
            {
                // Same day range
                return currentTime >= startTime && currentTime < endTime;
            }
            else
            {
                // Overnight range (e.g., 9 PM to 1 AM)
                return currentTime >= startTime || currentTime < endTime;
            }
        }

        private void DisplayProfile(Profile profile)
        {
            // Update profile icon
            if (!string.IsNullOrEmpty(profile.Icon))
            {
                try
                {
                    ProfileIcon.Source = new BitmapImage(new Uri($"pack://application:,,,/{profile.Icon}"));
                }
                catch
                {
                    // Fallback if icon can't be loaded
                    ProfileIcon.Source = null;
                }
            }

            // Extract hex color from icon filename for styling
            var hexColor = ExtractHexFromIcon(profile.Icon);
            var colorBrush = GetBrushFromHex(hexColor);
            var darkerColorBrush = GetDarkerBrushFromHex(hexColor);

            // Update profile icon border color
            ProfileIconBorder.Background = colorBrush;

            // Update profile card background color (entire block) - darker version
            ProfileCard.Background = darkerColorBrush;
            ProfileCard.BorderBrush = null;
            ProfileCard.BorderThickness = new Thickness(0);

            // Keep time slot with original dark background
            TimeSlotBorder.Background = new SolidColorBrush(Color.FromRgb(20, 23, 32));
            TimeSlotBorder.BorderBrush = null;
            TimeSlotBorder.BorderThickness = new Thickness(0);

            // Update profile information
            ProfileName.Text = profile.Name;
            ProfileDescription.Text = profile.Description;
            TimeSlot.Text = $"{profile.StartTime} - {profile.EndTime}";

            // Calculate and display time remaining
            var timeRemaining = CalculateTimeRemaining(profile);
            TimeRemaining.Text = FormatTimeRemaining(timeRemaining);

            // Load and display tasks
            LoadTasks(profile, hexColor);
        }

        private string ExtractHexFromIcon(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath))
                return "3dbc93"; // Default color

            // Extract filename from path
            var filename = System.IO.Path.GetFileNameWithoutExtension(iconPath);
            
            // Check if filename is a valid hex color (6 characters)
            if (filename != null && filename.Length == 6 && IsValidHex(filename))
            {
                return filename;
            }

            return "3dbc93"; // Default color
        }

        private bool IsValidHex(string hex)
        {
            return hex.All(c => "0123456789abcdefABCDEF".Contains(c));
        }

        private SolidColorBrush GetBrushFromHex(string hex)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString($"#{hex}");
                return new SolidColorBrush(color);
            }
            catch
            {
                return new SolidColorBrush(Color.FromRgb(61, 188, 147)); // Default color
            }
        }

        private SolidColorBrush GetDarkerBrushFromHex(string hex)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString($"#{hex}");
                
                // Make the color darker by reducing RGB values by 30%
                var darkerColor = Color.FromRgb(
                    (byte)(color.R * 0.7),
                    (byte)(color.G * 0.7),
                    (byte)(color.B * 0.7)
                );
                
                return new SolidColorBrush(darkerColor);
            }
            catch
            {
                return new SolidColorBrush(Color.FromRgb(43, 132, 103)); // Default darker color
            }
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
                // Add 24 hours to end time for calculation
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
                if (minutes == 1)
                    return "1 minute remaining";
                else
                    return $"{minutes} minutes remaining";
            }
            else
            {
                var hours = (int)timeRemaining.TotalHours;
                var minutes = timeRemaining.Minutes;
                
                if (hours == 1 && minutes == 0)
                    return "1 hour remaining";
                else if (hours == 1 && minutes == 1)
                    return "1 hour and 1 minute remaining";
                else if (hours == 1)
                    return $"1 hour and {minutes} minutes remaining";
                else if (minutes == 0)
                    return $"{hours} hours remaining";
                else if (minutes == 1)
                    return $"{hours} hours and 1 minute remaining";
                else
                    return $"{hours} hours and {minutes} minutes remaining";
            }
        }

        private void LoadTasks(Profile profile, string hexColor)
        {
            TasksPanel.Children.Clear();

            if (profile.Tasks == null || !profile.Tasks.Any())
            {
                var noTasksText = new TextBlock
                {
                    Text = "No tasks for this profile",
                    Foreground = Brushes.LightGray,
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                TasksPanel.Children.Add(noTasksText);
                return;
            }

            var colorBrush = GetBrushFromHex(hexColor);

            foreach (var task in profile.Tasks)
            {
                CreateTaskElement(task.Trim(), colorBrush);
            }
        }

        private void CreateTaskElement(string taskText, SolidColorBrush accentColor)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 35, 41)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Task indicator dot
            var dot = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = accentColor,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(dot, 0);

            // Task text
            var taskLabel = new TextBlock
            {
                Text = taskText,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(12, 0, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(taskLabel, 1);

            grid.Children.Add(dot);
            grid.Children.Add(taskLabel);
            border.Child = grid;

            TasksPanel.Children.Add(border);
        }

        private void ShowNoProfileMessage()
        {
            ProfileName.Text = "No Active Profile";
            ProfileDescription.Text = "No profile is currently active for this time slot.";
            TimeSlot.Text = "--:-- - --:--";
            TimeRemaining.Text = "No time remaining";
            ProfileIcon.Source = null;
            
            // Reset to default colors
            var defaultBrush = new SolidColorBrush(Color.FromRgb(45, 55, 72));
            ProfileIconBorder.Background = defaultBrush;
            ProfileCard.Background = new SolidColorBrush(Color.FromRgb(20, 23, 32)); // Default card background
            ProfileCard.BorderBrush = null;
            ProfileCard.BorderThickness = new Thickness(0);
            TimeSlotBorder.Background = new SolidColorBrush(Color.FromRgb(20, 23, 32)); // Default time slot background
            TimeSlotBorder.BorderBrush = null;
            TimeSlotBorder.BorderThickness = new Thickness(0);
            
            TasksPanel.Children.Clear();

            var noProfileText = new TextBlock
            {
                Text = "No active profile",
                Foreground = Brushes.LightGray,
                FontStyle = FontStyles.Italic,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            TasksPanel.Children.Add(noProfileText);
        }
    }
}
