using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace SPMWPF.Models
{
    public class Project
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ColorValue { get; set; } = "#32CD32"; // Store color as hex string
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Helper property to convert string color to SolidColorBrush
        public SolidColorBrush Color
        {
            get
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(ColorValue));
                }
                catch
                {
                    return new SolidColorBrush(Colors.LimeGreen);
                }
            }
            set
            {
                ColorValue = value.Color.ToString();
            }
        }

        // Helper property for duration display
        public string Duration => $"{StartDate:MMM d, yyyy} - {EndDate:MMM d, yyyy}";
    }

    public static class ProjectDataService
    {
        private static readonly string ProjectsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "projects.csv");

        public static List<Project> LoadProjects()
        {
            var projects = new List<Project>();

            if (!File.Exists(ProjectsFilePath))
            {
                return projects;
            }

            try
            {
                var lines = File.ReadAllLines(ProjectsFilePath);
                
                // Skip header line if it exists
                var startIndex = lines.Length > 0 && lines[0].StartsWith("Id,") ? 1 : 0;
                
                for (int i = startIndex; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    var project = ParseProjectFromCsv(line);
                    if (project != null)
                    {
                        projects.Add(project);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if needed, but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Error loading projects: {ex.Message}");
            }

            return projects;
        }

        public static void SaveProjects(List<Project> projects)
        {
            try
            {
                var lines = new List<string>
                {
                    "Id,Name,StartDate,EndDate,Description,ColorValue,CreatedDate" // Header
                };

                foreach (var project in projects)
                {
                    lines.Add(FormatProjectToCsv(project));
                }

                File.WriteAllLines(ProjectsFilePath, lines);
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error saving projects: {ex.Message}");
            }
        }

        private static Project? ParseProjectFromCsv(string csvLine)
        {
            try
            {
                var fields = ParseCsvLine(csvLine);
                if (fields.Count < 7) return null;

                return new Project
                {
                    Id = fields[0],
                    Name = fields[1],
                    StartDate = DateTime.Parse(fields[2]),
                    EndDate = DateTime.Parse(fields[3]),
                    Description = fields[4],
                    ColorValue = fields[5],
                    CreatedDate = DateTime.Parse(fields[6])
                };
            }
            catch
            {
                return null;
            }
        }

        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var current = "";
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        current += '"';
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            fields.Add(current);
            return fields;
        }

        private static string FormatProjectToCsv(Project project)
        {
            return $"{EscapeCsvField(project.Id)}," +
                   $"{EscapeCsvField(project.Name)}," +
                   $"{EscapeCsvField(project.StartDate.ToString("yyyy-MM-dd"))}," +
                   $"{EscapeCsvField(project.EndDate.ToString("yyyy-MM-dd"))}," +
                   $"{EscapeCsvField(project.Description)}," +
                   $"{EscapeCsvField(project.ColorValue)}," +
                   $"{EscapeCsvField(project.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"))}";
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // If field contains comma, quote, or newline, wrap in quotes and escape quotes
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return '"' + field.Replace("\"", "\"\"") + '"';
            }

            return field;
        }
    }
}
