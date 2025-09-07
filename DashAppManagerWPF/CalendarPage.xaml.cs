using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DashAppManagerWPF
{
    public partial class CalendarPage : UserControl
    {
        private CalendarView? calendarView;
        private static readonly SolidColorBrush[] ProjectColors = new[]
        {
            new SolidColorBrush(Colors.LimeGreen),
            new SolidColorBrush(Colors.DeepSkyBlue),
            new SolidColorBrush(Colors.Orange),
            new SolidColorBrush(Colors.MediumOrchid),
            new SolidColorBrush(Colors.Tomato)
        };
        private int colorIndex = 0;

        public CalendarPage()
        {
            InitializeComponent();
            Loaded += CalendarPage_Loaded;
        }

        private void CalendarPage_Loaded(object sender, RoutedEventArgs e)
        {
            calendarView = this.FindName("CalendarView") as CalendarView;
            if (calendarView == null)
            {
                // Find the CalendarView in the visual tree
                calendarView = FindVisualChild<CalendarView>(this);
            }

            if (calendarView != null)
            {
                calendarView.OnDayClicked = OnDayClicked;
                calendarView.OnAddProject = OnAddProject;
                
                // Add some sample projects for demo
                AddSampleProjects();
            }
        }

        private void AddSampleProjects()
        {
            if (calendarView == null) return;

            var sampleProjects = new[]
            {
                new Project 
                { 
                    Name = "Website Redesign", 
                    StartDate = new DateTime(2025, 9, 5), 
                    EndDate = new DateTime(2025, 9, 15),
                    Description = "Complete redesign of the company website with modern UI/UX principles",
                    Color = new SolidColorBrush(Colors.LimeGreen)
                },
                new Project 
                { 
                    Name = "Mobile App Development", 
                    StartDate = new DateTime(2025, 9, 10), 
                    EndDate = new DateTime(2025, 9, 25),
                    Description = "Develop a cross-platform mobile application for our services",
                    Color = new SolidColorBrush(Colors.DeepSkyBlue)
                },
                new Project 
                { 
                    Name = "Database Migration", 
                    StartDate = new DateTime(2025, 9, 20), 
                    EndDate = new DateTime(2025, 9, 22),
                    Description = "Migrate existing database to new cloud infrastructure",
                    Color = new SolidColorBrush(Colors.Orange)
                }
            };

            foreach (var project in sampleProjects)
            {
                calendarView.AddProject(project);
            }
        }

        private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                    return (T)child;
                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void OnDayClicked(DateTime date, List<Project> projects)
        {
            if (projects.Any())
            {
                ShowProjectDetails(projects.First());
            }
            else
            {
                ShowDefaultContent();
            }
        }

        private void OnAddProject()
        {
            ShowProjectCreationForm();
        }

        private void ShowDefaultContent()
        {
            DefaultContent.Visibility = Visibility.Visible;
            ProjectCreationForm.Visibility = Visibility.Collapsed;
            ProjectDetails.Visibility = Visibility.Collapsed;
        }

        private void ShowProjectCreationForm()
        {
            DefaultContent.Visibility = Visibility.Collapsed;
            ProjectCreationForm.Visibility = Visibility.Visible;
            ProjectDetails.Visibility = Visibility.Collapsed;
            
            // Clear form
            ProjectNameTextBox.Text = "";
            ProjectStartDatePicker.SelectedDate = DateTime.Today;
            ProjectEndDatePicker.SelectedDate = DateTime.Today.AddDays(7);
            ProjectDescriptionTextBox.Text = "";
        }

        private void ShowProjectDetails(Project project)
        {
            DefaultContent.Visibility = Visibility.Collapsed;
            ProjectCreationForm.Visibility = Visibility.Collapsed;
            ProjectDetails.Visibility = Visibility.Visible;
            
            ProjectDetailsName.Text = project.Name;
            ProjectDetailsDuration.Text = $"{project.StartDate:MMM d, yyyy} - {project.EndDate:MMM d, yyyy}";
            ProjectDetailsDescription.Text = project.Description;
        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text) ||
                ProjectStartDatePicker.SelectedDate == null ||
                ProjectEndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var project = new Project
            {
                Name = ProjectNameTextBox.Text,
                StartDate = ProjectStartDatePicker.SelectedDate.Value,
                EndDate = ProjectEndDatePicker.SelectedDate.Value,
                Description = ProjectDescriptionTextBox.Text,
                Color = ProjectColors[colorIndex % ProjectColors.Length]
            };

            colorIndex++;

            calendarView?.AddProject(project);
            ShowDefaultContent();
        }

        private void CancelProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDefaultContent();
        }

        private void CloseDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDefaultContent();
        }
    }
}
