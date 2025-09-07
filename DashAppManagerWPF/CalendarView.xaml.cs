using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DashAppManagerWPF
{
    public class Project
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public SolidColorBrush Color { get; set; } = new SolidColorBrush(Colors.Blue);
    }

    public partial class CalendarView : UserControl
    {
        public static readonly DependencyProperty OnDayClickedProperty =
            DependencyProperty.Register("OnDayClicked", typeof(Action<DateTime, List<Project>>), typeof(CalendarView));

        public Action<DateTime, List<Project>> OnDayClicked
        {
            get { return (Action<DateTime, List<Project>>)GetValue(OnDayClickedProperty); }
            set { SetValue(OnDayClickedProperty, value); }
        }

        public static readonly DependencyProperty OnAddProjectProperty =
            DependencyProperty.Register("OnAddProject", typeof(Action), typeof(CalendarView));

        public Action OnAddProject
        {
            get { return (Action)GetValue(OnAddProjectProperty); }
            set { SetValue(OnAddProjectProperty, value); }
        }

        private List<Project> projects = new List<Project>();
        private DateTime currentDate = DateTime.Now;

        public CalendarView()
        {
            InitializeComponent();
            Loaded += CalendarView_Loaded;
        }

        private void CalendarView_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateCalendar(currentDate.Year, currentDate.Month);
            UpdateMonthYearDisplay();
        }

        private void UpdateMonthYearDisplay()
        {
            MonthYearTextBlock.Text = $"{currentDate.ToString("MMMM", CultureInfo.InvariantCulture)}, {currentDate.Year}";
        }

        public void AddProject(Project project)
        {
            projects.Add(project);
            PopulateCalendar(currentDate.Year, currentDate.Month);
        }

        private void PopulateCalendar(int year, int month)
        {
            CalendarGrid.Children.Clear();
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            // Add empty cells for days before the first day of the month
            for (int i = 0; i < startDayOfWeek; i++)
            {
                var emptyCell = new Border();
                Grid.SetRow(emptyCell, 0);
                Grid.SetColumn(emptyCell, i);
                CalendarGrid.Children.Add(emptyCell);
            }

            // Add cells for each day of the month
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDay = new DateTime(year, month, day);
                var dayProjects = projects.Where(p => currentDay >= p.StartDate && currentDay <= p.EndDate).ToList();

                var position = day - 1 + startDayOfWeek;
                var row = position / 7;
                var col = position % 7;

                var dayCell = new Border
                {
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(2),
                    CornerRadius = new CornerRadius(8),
                    Cursor = Cursors.Hand,
                    Tag = currentDay
                };

                // Add hover effects
                dayCell.MouseEnter += (s, e) =>
                {
                    dayCell.Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));
                };

                dayCell.MouseLeave += (s, e) =>
                {
                    dayCell.Background = Brushes.Transparent;
                };

                dayCell.MouseLeftButtonDown += (s, e) =>
                {
                    OnDayClicked?.Invoke(currentDay, dayProjects);
                };

                var stackPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top
                };

                var dayTextBlock = new TextBlock
                {
                    Text = day.ToString(),
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.Medium,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                stackPanel.Children.Add(dayTextBlock);
                dayCell.Child = stackPanel;

                Grid.SetRow(dayCell, row);
                Grid.SetColumn(dayCell, col);
                CalendarGrid.Children.Add(dayCell);
            }

            // Add project bars after all day cells are created
            AddProjectBars(year, month, startDayOfWeek, daysInMonth);
        }

        private void AddProjectBars(int year, int month, int startDayOfWeek, int daysInMonth)
        {
            var projectsByRow = new Dictionary<int, List<Project>>();

            // Group projects by their visual row to avoid overlaps
            foreach (var project in projects)
            {
                var projectStartInMonth = project.StartDate.Month == month && project.StartDate.Year == year ? project.StartDate.Day : 1;
                var projectEndInMonth = project.EndDate.Month == month && project.EndDate.Year == year ? project.EndDate.Day : daysInMonth;

                // Skip projects that don't intersect with current month
                if (project.EndDate < new DateTime(year, month, 1) || project.StartDate > new DateTime(year, month, daysInMonth))
                    continue;

                // Find a suitable row for this project
                int row = 0;
                while (projectsByRow.ContainsKey(row))
                {
                    bool hasConflict = projectsByRow[row].Any(p =>
                    {
                        var pStartInMonth = p.StartDate.Month == month && p.StartDate.Year == year ? p.StartDate.Day : 1;
                        var pEndInMonth = p.EndDate.Month == month && p.EndDate.Year == year ? p.EndDate.Day : daysInMonth;
                        return !(projectEndInMonth < pStartInMonth || projectStartInMonth > pEndInMonth);
                    });

                    if (!hasConflict) break;
                    row++;
                }

                if (!projectsByRow.ContainsKey(row))
                    projectsByRow[row] = new List<Project>();
                projectsByRow[row].Add(project);

                // Create the project bar
                CreateProjectBar(project, projectStartInMonth, projectEndInMonth, startDayOfWeek, row);
            }
        }

        private void CreateProjectBar(Project project, int startDay, int endDay, int startDayOfWeek, int row)
        {
            // Calculate grid position
            var startPosition = startDay - 1 + startDayOfWeek;
            var duration = endDay - startDay + 1;
            var gridRow = startPosition / 7;
            var gridCol = startPosition % 7;
            var endPosition = startPosition + duration - 1;
            var endGridRow = endPosition / 7;

            // Handle multi-week projects by creating segments
            while (gridRow <= endGridRow)
            {
                var segmentStartCol = gridRow == startPosition / 7 ? gridCol : 0;
                var segmentEndCol = gridRow == endGridRow ? endPosition % 7 : 6;
                var segmentDuration = segmentEndCol - segmentStartCol + 1;

                var projectBar = new Border
                {
                    Background = project.Color,
                    CornerRadius = new CornerRadius(10),
                    Height = 16,
                    Margin = new Thickness(4, 25 + (row * 18), 4, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    IsHitTestVisible = false
                };

                var projectText = new TextBlock
                {
                    Text = project.Name,
                    Foreground = Brushes.White,
                    FontSize = 9,
                    FontWeight = FontWeights.Medium,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(6, 0, 6, 0),
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                projectBar.Child = projectText;

                // Set grid position
                Grid.SetRow(projectBar, gridRow);
                Grid.SetColumn(projectBar, segmentStartCol);
                Grid.SetColumnSpan(projectBar, segmentDuration);

                CalendarGrid.Children.Add(projectBar);
                gridRow++;
            }
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            OnAddProject?.Invoke();
        }
    }
}
