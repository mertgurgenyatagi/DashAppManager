using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DashAppManagerWPF.Models;

namespace DashAppManagerWPF
{
    public partial class NotificationsPage : UserControl
    {
        private List<Profile>? _profiles;
        private List<TaskItem>? _allTasks;
        private HashSet<string>? _completedTaskIds;
        private static readonly string CompletedTasksFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "completed_tasks.txt");

        public NotificationsPage()
        {
            InitializeComponent();
            LoadData();
            LoadTasks();
        }

        private void LoadData()
        {
            try
            {
                _profiles = ProfileDataService.LoadProfiles();
                _completedTaskIds = LoadCompletedTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
                _profiles = new List<Profile>();
                _completedTaskIds = new HashSet<string>();
            }
        }

        private HashSet<string> LoadCompletedTasks()
        {
            var completedTasks = new HashSet<string>();
            try
            {
                if (File.Exists(CompletedTasksFilePath))
                {
                    var lines = File.ReadAllLines(CompletedTasksFilePath);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            completedTasks.Add(line.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading completed tasks: {ex.Message}");
            }
            return completedTasks;
        }

        private void SaveCompletedTasks()
        {
            try
            {
                if (_completedTaskIds != null)
                {
                    File.WriteAllLines(CompletedTasksFilePath, _completedTaskIds);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving completed tasks: {ex.Message}");
            }
        }

        private void LoadTasks()
        {
            _allTasks = new List<TaskItem>();
            if (TaskListPanel != null)
            {
                TaskListPanel.Children.Clear();
            }

            if (_profiles == null || !_profiles.Any())
                return;

            // Get current profile to determine stopping point
            var currentProfile = GetCurrentProfile();
            var currentProfileIndex = currentProfile != null ? GetProfileIndex(currentProfile) : -1;

            // Process profiles from 00:00 onward up to current profile
            for (int i = 0; i <= currentProfileIndex && i < _profiles.Count; i++)
            {
                var profile = _profiles[i];
                var hexColor = ExtractHexFromIcon(profile.Icon);
                var profileName = profile.Name;

                if (profile.Tasks != null && profile.Tasks.Any())
                {
                    foreach (var task in profile.Tasks)
                    {
                        var taskId = $"{profile.Id}_{task}";
                        var isCompleted = _completedTaskIds?.Contains(taskId) ?? false;
                        
                        var taskItem = new TaskItem
                        {
                            Id = taskId,
                            Text = task,
                            ProfileName = profileName,
                            HexColor = hexColor,
                            IsCompleted = isCompleted
                        };
                        
                        _allTasks.Add(taskItem);
                    }
                }
            }

            // Sort tasks: incomplete first, then completed
            var sortedTasks = _allTasks.OrderBy(t => t.IsCompleted).ToList();
            
            foreach (var task in sortedTasks)
            {
                CreateTaskElement(task);
            }
        }

        private Profile? GetCurrentProfile()
        {
            if (_profiles == null)
                return null;

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            foreach (var profile in _profiles)
            {
                var startTime = ParseTimeString(profile.StartTime);
                var endTime = ParseTimeString(profile.EndTime);
                
                if (startTime.HasValue && endTime.HasValue && IsTimeInRange(currentTime, startTime.Value, endTime.Value))
                {
                    return profile;
                }
            }

            return null;
        }

        private int GetProfileIndex(Profile targetProfile)
        {
            if (_profiles == null)
                return -1;

            for (int i = 0; i < _profiles.Count; i++)
            {
                if (_profiles[i].Id == targetProfile.Id)
                    return i;
            }
            return -1;
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
                return "3dbc93";

            var filename = System.IO.Path.GetFileNameWithoutExtension(iconPath);
            
            if (filename != null && filename.Length == 6 && IsValidHex(filename))
            {
                return filename;
            }

            return "3dbc93";
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
                return new SolidColorBrush(Color.FromRgb(61, 188, 147));
            }
        }

        private void CreateTaskElement(TaskItem taskItem)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 35, 41)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10),
                Opacity = taskItem.IsCompleted ? 0.5 : 1.0
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Task completion button (circle)
            var button = CreateTaskButton(taskItem);
            Grid.SetColumn(button, 0);

            // Task text
            var taskText = new TextBlock
            {
                Text = taskItem.Text,
                Foreground = taskItem.IsCompleted ? Brushes.Gray : Brushes.White,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(12, 0, 12, 0),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(taskText, 1);

            // Profile name
            var profileLabel = new TextBlock
            {
                Text = taskItem.ProfileName,
                Foreground = taskItem.IsCompleted ? Brushes.Gray : GetBrushFromHex(taskItem.HexColor),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right
            };
            Grid.SetColumn(profileLabel, 2);

            grid.Children.Add(button);
            grid.Children.Add(taskText);
            grid.Children.Add(profileLabel);
            border.Child = grid;

            if (TaskListPanel != null)
            {
                TaskListPanel.Children.Add(border);
            }
        }

        private Button CreateTaskButton(TaskItem taskItem)
        {
            var button = new Button
            {
                Width = 20,
                Height = 20,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                Style = null,
                Template = CreateButtonTemplate(taskItem)
            };

            button.Click += (s, e) => OnTaskButtonClick(taskItem, button);
            return button;
        }

        private ControlTemplate CreateButtonTemplate(TaskItem taskItem)
        {
            var template = new ControlTemplate(typeof(Button));
            
            var grid = new FrameworkElementFactory(typeof(Grid));
            template.VisualTree = grid;

            // Background circle
            var ellipse = new FrameworkElementFactory(typeof(Ellipse));
            ellipse.SetValue(Ellipse.WidthProperty, 20.0);
            ellipse.SetValue(Ellipse.HeightProperty, 20.0);
            ellipse.SetValue(Ellipse.StrokeProperty, GetBrushFromHex(taskItem.HexColor));
            ellipse.SetValue(Ellipse.StrokeThicknessProperty, 2.0);
            ellipse.SetValue(Ellipse.FillProperty, taskItem.IsCompleted ? GetBrushFromHex(taskItem.HexColor) : Brushes.Transparent);
            ellipse.Name = "BackgroundEllipse";
            grid.AppendChild(ellipse);

            // Checkmark (visible when completed or on hover)
            var checkmark = new FrameworkElementFactory(typeof(TextBlock));
            checkmark.SetValue(TextBlock.TextProperty, "✓");
            checkmark.SetValue(TextBlock.ForegroundProperty, taskItem.IsCompleted ? Brushes.White : GetBrushFromHex(taskItem.HexColor));
            checkmark.SetValue(TextBlock.FontSizeProperty, 12.0);
            checkmark.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            checkmark.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            checkmark.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            checkmark.SetValue(TextBlock.OpacityProperty, taskItem.IsCompleted ? 1.0 : 0.0);
            checkmark.Name = "Checkmark";
            grid.AppendChild(checkmark);

            // Hover triggers
            var trigger = new Trigger();
            trigger.Property = Button.IsMouseOverProperty;
            trigger.Value = true;
            
            var fadeInAnimation = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150));
            var checkmarkFadeIn = new Setter(TextBlock.OpacityProperty, 1.0);
            checkmarkFadeIn.TargetName = "Checkmark";
            trigger.Setters.Add(checkmarkFadeIn);

            template.Triggers.Add(trigger);

            return template;
        }

        private void OnTaskButtonClick(TaskItem taskItem, Button button)
        {
            if (!taskItem.IsCompleted)
            {
                // Mark as completed
                taskItem.IsCompleted = true;
                if (_completedTaskIds != null)
                {
                    _completedTaskIds.Add(taskItem.Id);
                }
                SaveCompletedTasks();

                // Refresh the task list to move completed task to bottom
                LoadTasks();
            }
        }

        public class TaskItem
        {
            public string Id { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
            public string ProfileName { get; set; } = string.Empty;
            public string HexColor { get; set; } = string.Empty;
            public bool IsCompleted { get; set; }
        }
    }
}
