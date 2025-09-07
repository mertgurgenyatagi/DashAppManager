using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DashAppManagerWPF.Models;

namespace DashAppManagerWPF
{
    public partial class CalendarPage : UserControl
    {
        private CalendarView? calendarView;
        private List<Project> projects = new List<Project>();
        private static readonly string[] ProjectColorValues = new[]
        {
            "#32CD32", // LimeGreen
            "#00BFFF", // DeepSkyBlue
            "#FFA500", // Orange
            "#BA55D3", // MediumOrchid
            "#FF6347"  // Tomato
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
                
                // Load projects from CSV
                LoadProjects();
            }
        }

        private void LoadProjects()
        {
            projects = ProjectDataService.LoadProjects();
            RefreshCalendar();
        }

        private void RefreshCalendar()
        {
            if (calendarView == null) return;

            // Clear existing projects and add loaded ones
            calendarView.ClearProjects();
            foreach (var project in projects)
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
                return; // Simply don't create if validation fails
            }

            var project = new Project
            {
                Id = Guid.NewGuid().ToString(),
                Name = ProjectNameTextBox.Text,
                StartDate = ProjectStartDatePicker.SelectedDate.Value,
                EndDate = ProjectEndDatePicker.SelectedDate.Value,
                Description = ProjectDescriptionTextBox.Text,
                ColorValue = ProjectColorValues[colorIndex % ProjectColorValues.Length]
            };

            colorIndex++;

            // Add to local list and save to CSV
            projects.Add(project);
            ProjectDataService.SaveProjects(projects);

            // Add to calendar view
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
