using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;
using System.Collections.Generic;

namespace DashAppManagerWPF
{
    public partial class ConfigPage : UserControl
    {
        private ObservableCollection<string> currentTasks = new ObservableCollection<string>();
        private string selectedColor = "#FF6347"; // Default color
        private string? editingProfileId = null; // Track which profile is being edited

        public ConfigPage()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            TasksItemsControl.ItemsSource = currentTasks;
            
            // Initialize time slots
            PopulateTimeSlots();
            
            // Set default selected color
            UpdateColorSelection(ColorOption1);
        }

        private void PopulateTimeSlots()
        {
            var timeSlots = new List<string>();
            
            // Generate time slots from 6:00 AM to 11:30 PM in 30-minute intervals
            for (int hour = 6; hour <= 23; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    string period = hour < 12 ? "AM" : "PM";
                    int displayHour = hour > 12 ? hour - 12 : (hour == 0 ? 12 : hour);
                    timeSlots.Add($"{displayHour:D2}:{minute:D2} {period}");
                }
            }
            
            StartTimeComboBox.ItemsSource = timeSlots;
            EndTimeComboBox.ItemsSource = timeSlots;
            
            // Set default values
            StartTimeComboBox.SelectedItem = "08:30 AM";
            EndTimeComboBox.SelectedItem = "12:00 PM";
        }

        private void UpdateColorSelection(Border selectedBorder)
        {
            // Reset all color options
            if (selectedBorder.Parent is StackPanel parentPanel)
            {
                foreach (var child in parentPanel.Children)
                {
                    if (child is Border border)
                    {
                        border.BorderThickness = new Thickness(0);
                        border.BorderBrush = null;
                    }
                }
            }
            
            // Highlight selected color
            selectedBorder.BorderThickness = new Thickness(3);
            selectedBorder.BorderBrush = new SolidColorBrush(Colors.White);
            selectedColor = selectedBorder.Tag?.ToString() ?? "#FF6347";
        }

        private void ColorOption_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                UpdateColorSelection(border);
            }
        }

        private void NewProfile_Click(object sender, MouseButtonEventArgs e)
        {
            ShowProfileForm(true);
        }

        private void Profile_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border profileBorder && profileBorder.Tag != null)
            {
                editingProfileId = profileBorder.Tag.ToString();
                LoadProfileForEditing(profileBorder);
                ShowProfileForm(false);
            }
        }

        private void LoadProfileForEditing(Border profileBorder)
        {
            // Find the text elements in the profile
            var grid = profileBorder.Child as Grid;
            var stackPanel = grid?.Children.OfType<StackPanel>().FirstOrDefault();
            var textBlocks = stackPanel?.Children.OfType<TextBlock>().ToList();
            
            if (textBlocks != null && textBlocks.Count >= 2)
            {
                // Load profile name
                ProfileNameTextBox.Text = textBlocks[0].Text;
                
                // Load time slot
                var timeSlot = textBlocks[1].Text;
                if (timeSlot.Contains(" - "))
                {
                    var times = timeSlot.Split(new[] { " - " }, System.StringSplitOptions.None);
                    StartTimeComboBox.SelectedItem = times[0];
                    if (times.Length > 1)
                        EndTimeComboBox.SelectedItem = times[1];
                }
                
                // Load color
                var ellipse = grid?.Children.OfType<Ellipse>().FirstOrDefault();
                if (ellipse?.Fill is SolidColorBrush brush)
                {
                    selectedColor = brush.Color.ToString();
                    // Find and select the matching color option
                    if (ColorOption1.Parent is StackPanel colorPanel)
                    {
                        var colorOptions = colorPanel.Children.OfType<Border>();
                        var matchingOption = colorOptions.FirstOrDefault(co => co.Tag?.ToString() == selectedColor);
                        if (matchingOption != null)
                        {
                            UpdateColorSelection(matchingOption);
                        }
                    }
                }
                
                // Load sample tasks (for demo purposes)
                currentTasks.Clear();
                currentTasks.Add("Review quarterly reports");
                currentTasks.Add("Team meeting preparation");
                currentTasks.Add("Client presentation");
            }
        }

        private void ShowProfileForm(bool isNewProfile)
        {
            if (isNewProfile)
            {
                // Reset form for new profile
                FormTitle.Text = "Create New Profile";
                SaveProfileButton.Content = "Create Profile";
                DeleteProfileButton.Visibility = Visibility.Collapsed;
                
                ProfileNameTextBox.Text = "Enter profile name...";
                StartTimeComboBox.SelectedItem = "08:30 AM";
                EndTimeComboBox.SelectedItem = "12:00 PM";
                UpdateColorSelection(ColorOption1);
                currentTasks.Clear();
                editingProfileId = null;
            }
            else
            {
                // Setup form for editing
                FormTitle.Text = "Edit Profile";
                SaveProfileButton.Content = "Save Changes";
                DeleteProfileButton.Visibility = Visibility.Visible;
            }
            
            WelcomePanel.Visibility = Visibility.Collapsed;
            ProfileFormScrollViewer.Visibility = Visibility.Visible;
        }

        private void HideProfileForm()
        {
            ProfileFormScrollViewer.Visibility = Visibility.Collapsed;
            WelcomePanel.Visibility = Visibility.Visible;
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string taskText = NewTaskTextBox.Text.Trim();
            
            if (!string.IsNullOrEmpty(taskText) && taskText != "Enter new task...")
            {
                currentTasks.Add(taskText);
                NewTaskTextBox.Text = "Enter new task...";
            }
        }

        private void RemoveTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string taskToRemove)
            {
                currentTasks.Remove(taskToRemove);
            }
        }

        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            string profileName = ProfileNameTextBox.Text.Trim();
            string? startTime = StartTimeComboBox.SelectedItem?.ToString();
            string? endTime = EndTimeComboBox.SelectedItem?.ToString();
            
            if (string.IsNullOrEmpty(profileName) || profileName == "Enter profile name...")
            {
                MessageBox.Show("Please enter a valid profile name.", "Validation Error", 
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
            {
                MessageBox.Show("Please select both start and end times.", "Validation Error", 
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (editingProfileId != null)
            {
                // Update existing profile
                UpdateExistingProfile(profileName, startTime, endTime);
                MessageBox.Show("Profile updated successfully!", "Success", 
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Create new profile logic would go here
                // For demo purposes, we'll show a success message
                MessageBox.Show($"Profile '{profileName}' created successfully!\n" +
                               $"Time Slot: {startTime} - {endTime}\n" +
                               $"Color: {selectedColor}\n" +
                               $"Tasks: {currentTasks.Count} added", 
                               "Profile Created", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            
            HideProfileForm();
        }

        private void UpdateExistingProfile(string profileName, string startTime, string endTime)
        {
            // Find the profile being edited
            var profileBorders = ProfileGrid.Children.OfType<Border>()
                .Where(b => b.Tag?.ToString() == editingProfileId);
            
            foreach (var profileBorder in profileBorders)
            {
                var grid = profileBorder.Child as Grid;
                var stackPanel = grid?.Children.OfType<StackPanel>().FirstOrDefault();
                var textBlocks = stackPanel?.Children.OfType<TextBlock>().ToList();
                var ellipse = grid?.Children.OfType<Ellipse>().FirstOrDefault();
                
                if (textBlocks != null && textBlocks.Count >= 2 && ellipse != null)
                {
                    // Update profile name
                    textBlocks[0].Text = profileName;
                    
                    // Update time slot
                    textBlocks[1].Text = $"{startTime} - {endTime}";
                    
                    // Update color
                    var converter = new BrushConverter();
                    if (converter.ConvertFromString(selectedColor) is Brush brush)
                    {
                        ellipse.Fill = brush;
                    }
                }
            }
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to delete this profile?", 
                                       "Confirm Deletion", MessageBoxButton.YesNo, 
                                       MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                // Find and remove the profile being edited
                var profileToRemove = ProfileGrid.Children.OfType<Border>()
                    .FirstOrDefault(b => b.Tag?.ToString() == editingProfileId);
                
                if (profileToRemove != null)
                {
                    ProfileGrid.Children.Remove(profileToRemove);
                    MessageBox.Show("Profile deleted successfully!", "Success", 
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                HideProfileForm();
            }
        }

        private void CancelProfile_Click(object sender, RoutedEventArgs e)
        {
            HideProfileForm();
        }

        // Handle text box focus events for better UX
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
                AddTask_Click(sender, new RoutedEventArgs());
            }
        }
    }
}
