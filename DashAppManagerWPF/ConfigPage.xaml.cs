using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DashAppManagerWPF.Models;

namespace DashAppManagerWPF
{
    public partial class ConfigPage : UserControl
    {
        private List<Profile> profiles = new List<Profile>();
        private ObservableCollection<string> currentTasks = new ObservableCollection<string>();
        private string selectedIcon = "assets/icons/3dbc93.png"; // Default icon
        private string? editingProfileId = null; // Track which profile is being edited

        public ConfigPage()
        {
            InitializeComponent();
            LoadProfiles();
            InitializeForm();
        }

        private void LoadProfiles()
        {
            profiles = ProfileDataService.LoadProfiles();
            RefreshProfileGrid();
        }

        private void InitializeForm()
        {
            TasksItemsControl.ItemsSource = currentTasks;

            // Initialize time slots
            PopulateTimeSlots();

            // Set default selected icon
            UpdateIconSelection(IconOption1);
        }

        private void PopulateTimeSlots()
        {
            var timeSlots = new List<string>();

            // Generate time slots from 00:00 to 23:30 in 30-minute intervals
            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    timeSlots.Add($"{hour:D2}:{minute:D2}");
                }
            }

            StartTimeComboBox.ItemsSource = timeSlots;
            EndTimeComboBox.ItemsSource = timeSlots;

            // Set default values
            StartTimeComboBox.SelectedItem = "08:30";
            EndTimeComboBox.SelectedItem = "12:00";
        }

        private void RefreshProfileGrid()
        {
            ProfileGrid.Children.Clear();
            
            // Add existing profiles
            foreach (var profile in profiles)
            {
                AddProfileCard(profile);
            }
            
            // Add "New Profile" button
            AddNewProfileButton();
        }

        private void AddProfileCard(Profile profile)
        {
            var border = new Border 
            { 
                Height = 92, 
                Cursor = Cursors.Hand, 
                Tag = profile.Id,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141720")),
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(5)
            };

            // Add hover effects
            border.MouseEnter += (s, e) => border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e2329"));
            border.MouseLeave += (s, e) => border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#141720"));

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Icon Image
            var iconImage = new Image
            {
                Width = 32, Height = 32,
                Source = new BitmapImage(new Uri(profile.Icon, UriKind.Relative)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0)
            };
            Grid.SetColumn(iconImage, 0);

            // Text Content
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(15, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(stackPanel, 1);

            stackPanel.Children.Add(new TextBlock 
            { 
                Text = profile.Name, 
                Foreground = Brushes.White, 
                FontWeight = FontWeights.Bold, 
                FontSize = 14 
            });
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = profile.TimeSlot, 
                Foreground = Brushes.White, 
                FontStyle = FontStyles.Italic, 
                FontSize = 12 
            });
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Click to edit", 
                Foreground = Brushes.Gray, 
                FontSize = 10 
            });

            grid.Children.Add(iconImage); // Changed from ellipse to iconImage
            grid.Children.Add(stackPanel);
            border.Child = grid;
            border.MouseLeftButtonUp += Profile_Click;
            ProfileGrid.Children.Add(border);
        }

        private void AddNewProfileButton()
        {
            var border = new Border
            {
                Height = 92,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e2329")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333842")),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(12),
                Cursor = Cursors.Hand,
                Margin = new Thickness(5)
            };

            // Add hover effects
            border.MouseEnter += (s, e) => 
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252b33"));
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4a5259"));
            };
            border.MouseLeave += (s, e) => 
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e2329"));
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333842"));
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Circular Plus Frame
            var plusBorder = new Border
            {
                Width = 40, Height = 40,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3a4149")),
                CornerRadius = new CornerRadius(20),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            var plusText = new TextBlock
            {
                Text = "+",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7a8189")),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            plusBorder.Child = plusText;
            Grid.SetColumn(plusBorder, 0);

            // Text Content
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(15, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(stackPanel, 1);

            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "New Profile", 
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7a8189")), 
                FontWeight = FontWeights.Bold, 
                FontSize = 14 
            });
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Click to create", 
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5a6169")), 
                FontStyle = FontStyles.Italic, 
                FontSize = 12 
            });

            grid.Children.Add(plusBorder);
            grid.Children.Add(stackPanel);
            border.Child = grid;
            border.MouseLeftButtonUp += NewProfile_Click;
            ProfileGrid.Children.Add(border);
        }

        // Event Handlers
        private void Profile_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag != null)
            {
                var profileId = border.Tag.ToString();
                var profile = profiles.FirstOrDefault(p => p.Id == profileId);
                if (profile != null)
                {
                    EditProfile(profile);
                }
            }
        }

        private void NewProfile_Click(object sender, MouseButtonEventArgs e)
        {
            ShowProfileForm(isEditing: false);
        }

        private void EditProfile(Profile profile)
        {
            editingProfileId = profile.Id;
            
            // Populate form with profile data
            ProfileNameTextBox.Text = profile.Name;
            ProfileDescriptionTextBox.Text = profile.Description;
            StartTimeComboBox.SelectedItem = profile.StartTime;
            EndTimeComboBox.SelectedItem = profile.EndTime;
            
            // Set selected icon
            var iconBorder = FindIconBorder(profile.Icon);
            if (iconBorder != null)
            {
                UpdateIconSelection(iconBorder);
            }
            
            // Load tasks
            currentTasks.Clear();
            foreach (var task in profile.Tasks)
            {
                currentTasks.Add(task);
            }
            
            ShowProfileForm(isEditing: true);
        }

        private Border? FindIconBorder(string icon)
        {
            var iconBorders = new[] { IconOption1, IconOption2, IconOption3, IconOption4, 
                                     IconOption5, IconOption6 };
            return iconBorders.FirstOrDefault(b => b.Tag?.ToString() == icon);
        }

        private void ShowProfileForm(bool isEditing)
        {
            FormTitle.Text = isEditing ? "Edit Profile" : "Create New Profile";
            SaveProfileButton.Content = isEditing ? "Save Changes" : "Create Profile";
            DeleteProfileButton.Visibility = isEditing ? Visibility.Visible : Visibility.Collapsed;
            
            WelcomePanel.Visibility = Visibility.Collapsed;
            ProfileFormScrollViewer.Visibility = Visibility.Visible;
        }

        private void HideProfileForm()
        {
            WelcomePanel.Visibility = Visibility.Visible;
            ProfileFormScrollViewer.Visibility = Visibility.Collapsed;
            ClearForm();
        }

        private void ClearForm()
        {
            editingProfileId = null;
            ProfileNameTextBox.Text = "Enter profile name...";
            ProfileDescriptionTextBox.Text = "Enter role description...";
            StartTimeComboBox.SelectedItem = "08:30";
            EndTimeComboBox.SelectedItem = "12:00";
            currentTasks.Clear();
            UpdateIconSelection(IconOption1); // Changed from UpdateColorSelection
        }

        // Form Event Handlers
        private void ProfileNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ProfileNameTextBox.Text == "Enter profile name...")
            {
                ProfileNameTextBox.Text = "";
            }
        }

        private void ProfileNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProfileNameTextBox.Text))
            {
                ProfileNameTextBox.Text = "Enter profile name...";
            }
        }

        private void ProfileDescriptionTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ProfileDescriptionTextBox.Text == "Enter role description...")
            {
                ProfileDescriptionTextBox.Text = "";
            }
        }

        private void ProfileDescriptionTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProfileDescriptionTextBox.Text))
            {
                ProfileDescriptionTextBox.Text = "Enter role description...";
            }
        }

        private void NewTaskTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NewTaskTextBox.Text == "Enter new task...")
            {
                NewTaskTextBox.Text = "";
            }
        }

        private void NewTaskTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewTaskTextBox.Text))
            {
                NewTaskTextBox.Text = "Enter new task...";
            }
        }

        private void NewTaskTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddTask();
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            AddTask();
        }

        private void AddTask()
        {
            if (!string.IsNullOrWhiteSpace(NewTaskTextBox.Text) && NewTaskTextBox.Text != "Enter new task...")
            {
                currentTasks.Add(NewTaskTextBox.Text);
                NewTaskTextBox.Text = "Enter new task...";
            }
        }

        private void RemoveTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                var taskToRemove = button.Tag.ToString();
                currentTasks.Remove(taskToRemove);
            }
        }

        private void IconOption_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                UpdateIconSelection(border);
            }
        }

        private void UpdateIconSelection(Border selectedBorder)
        {
            // Reset all icon options
            var iconBorders = new[] { IconOption1, IconOption2, IconOption3, IconOption4, 
                                     IconOption5, IconOption6 };
            
            foreach (var border in iconBorders)
            {
                border.BorderThickness = new Thickness(0);
                border.BorderBrush = null;
            }

            // Highlight selected icon
            selectedBorder.BorderThickness = new Thickness(3);
            selectedBorder.BorderBrush = new SolidColorBrush(Colors.White);
            selectedIcon = selectedBorder.Tag?.ToString() ?? "assets/icons/3dbc93.png";
        }

        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            // Validate form (simplified - no popups)
            if (string.IsNullOrWhiteSpace(ProfileNameTextBox.Text) || ProfileNameTextBox.Text == "Enter profile name...")
            {
                return; // Simply don't save if invalid
            }

            if (StartTimeComboBox.SelectedItem == null || EndTimeComboBox.SelectedItem == null)
            {
                return; // Simply don't save if invalid
            }

            // Create or update profile
            Profile profile;
            if (editingProfileId != null)
            {
                // Update existing profile
                profile = profiles.First(p => p.Id == editingProfileId);
                profile.Name = ProfileNameTextBox.Text;
                profile.Description = string.IsNullOrWhiteSpace(ProfileDescriptionTextBox.Text) || ProfileDescriptionTextBox.Text == "Enter role description..." 
                    ? "" : ProfileDescriptionTextBox.Text;
                profile.Icon = selectedIcon; // Changed from Color to Icon
                profile.StartTime = StartTimeComboBox.SelectedItem.ToString();
                profile.EndTime = EndTimeComboBox.SelectedItem.ToString();
                profile.Tasks = currentTasks.ToList();
            }
            else
            {
                // Create new profile
                profile = new Profile
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = ProfileNameTextBox.Text,
                    Description = string.IsNullOrWhiteSpace(ProfileDescriptionTextBox.Text) || ProfileDescriptionTextBox.Text == "Enter role description..." 
                        ? "" : ProfileDescriptionTextBox.Text,
                    Icon = selectedIcon, // Changed from Color to Icon
                    StartTime = StartTimeComboBox.SelectedItem.ToString(),
                    EndTime = EndTimeComboBox.SelectedItem.ToString(),
                    Tasks = currentTasks.ToList()
                };
                profiles.Add(profile);
            }

            // Save to CSV
            ProfileDataService.SaveProfiles(profiles);
            
            // Refresh UI
            RefreshProfileGrid();
            HideProfileForm();
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (editingProfileId != null)
            {
                var profile = profiles.First(p => p.Id == editingProfileId);
                profiles.RemoveAll(p => p.Id == editingProfileId);
                ProfileDataService.SaveProfiles(profiles);
                RefreshProfileGrid();
                HideProfileForm();
            }
        }

        private void CancelProfile_Click(object sender, RoutedEventArgs e)
        {
            HideProfileForm();
        }
    }
}
