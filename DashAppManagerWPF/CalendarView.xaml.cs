using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DashAppManagerWPF
{
    public partial class CalendarView : UserControl
    {
        public CalendarView()
        {
            InitializeComponent();
            Loaded += CalendarView_Loaded;
        }

        private void CalendarView_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateCalendar(2025, 1);
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
                CalendarGrid.Children.Add(new Border());
            }

            // Add cells for each day of the month
            for (int day = 1; day <= daysInMonth; day++)
            {
                var dayCell = new Border
                {
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(2)
                };

                var stackPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var dayTextBlock = new TextBlock
                {
                    Text = day.ToString(),
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                stackPanel.Children.Add(dayTextBlock);
                dayCell.Child = stackPanel;
                CalendarGrid.Children.Add(dayCell);
            }
        }
    }
}
