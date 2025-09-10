using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SPMWPF
{
    public partial class NotesPage : UserControl
    {
        private Dictionary<DateTime, (string Title, string Body)> _notes = new();
        private DateTime _selectedDate;
        private List<DateTime> _dateList = new();
        private readonly string _csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "notes.csv");

        public NotesPage()
        {
            InitializeComponent();
            LoadNotesFromCsv();
            PopulateDates();
            SaveButton.Click += SaveButton_Click;
        }

        private void PopulateDates()
        {
            var startDate = new DateTime(2025, 9, 11);
            for (int i = 0; i < 100; i++)
            {
                var date = startDate.AddDays(i);
                _dateList.Add(date);
                var dayOfWeek = date.ToString("dddd", CultureInfo.InvariantCulture);
                var formatted = $"{dayOfWeek}, {date:MMMM dd, yyyy}";

                var btn = new Button
                {
                    Content = formatted,
                    Style = (Style)FindResource("DateButtonStyle"),
                    Tag = date
                };
                btn.Click += DateButton_Click;
                DatesStackPanel.Children.Add(btn);
            }
            // Select the first date by default
            if (_dateList.Count > 0)
                LoadNoteForDate(_dateList[0]);
        }

        private void DateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DateTime date)
            {
                SaveCurrentNote();
                LoadNoteForDate(date);
            }
        }

        private void LoadNoteForDate(DateTime date)
        {
            _selectedDate = date;
            if (_notes.TryGetValue(date, out var note))
            {
                TitleTextBox.Text = note.Title;
                BodyTextBox.Text = note.Body;
            }
            else
            {
                TitleTextBox.Text = "";
                BodyTextBox.Text = "";
            }
        }

        private void SaveCurrentNote()
        {
            if (_selectedDate != default)
            {
                _notes[_selectedDate] = (TitleTextBox.Text, BodyTextBox.Text);
                SaveNotesToCsv();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentNote();
        }

        private void LoadNotesFromCsv()
        {
            if (!File.Exists(_csvPath)) return;
            foreach (var line in File.ReadAllLines(_csvPath))
            {
                var parts = line.Split(new[] { ',' }, 3);
                if (parts.Length == 3 && DateTime.TryParse(parts[0], out var date))
                {
                    _notes[date] = (parts[1], parts[2]);
                }
            }
        }

        private void SaveNotesToCsv()
        {
            var lines = _notes.Select(kvp => $"{kvp.Key:yyyy-MM-dd},{EscapeCsv(kvp.Value.Title)},{EscapeCsv(kvp.Value.Body)}");
            File.WriteAllLines(_csvPath, lines);
        }

        private string EscapeCsv(string input)
        {
            if (input.Contains(",") || input.Contains("\n") || input.Contains("\r") || input.Contains("\""))
            {
                return $"\"{input.Replace("\"", "\"\"")}";
            }
            return input;
        }
    }
}
