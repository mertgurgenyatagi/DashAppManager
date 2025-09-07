using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DashAppManagerWPF.Models
{
    public class Profile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "assets/icons/3dbc93.png"; // Default to first colored icon
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public List<string> Tasks { get; set; } = new List<string>();
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string TimeSlot => !string.IsNullOrEmpty(StartTime) && !string.IsNullOrEmpty(EndTime) 
            ? $"{StartTime} - {EndTime}" 
            : string.Empty;

        public string TasksDisplay => Tasks.Any() ? string.Join(", ", Tasks) : "No tasks";
        
        // CSV Header for profile data
        public static string CsvHeader => "Id,Name,Description,Icon,StartTime,EndTime,Tasks";
    }

    public class ProfileDataService
    {
        private static readonly string CsvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles.csv");
        private const string CsvHeader = "Id,Name,Description,Icon,StartTime,EndTime,Tasks"; // Changed Color to Icon

        public static List<Profile> LoadProfiles()
        {
            var profiles = new List<Profile>();

            try
            {
                if (!File.Exists(CsvFilePath))
                {
                    // Create empty CSV file with header
                    File.WriteAllText(CsvFilePath, CsvHeader + Environment.NewLine);
                    return profiles;
                }

                var lines = File.ReadAllLines(CsvFilePath);
                
                // Skip header line
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    var profile = ParseProfileFromCsv(line);
                    if (profile != null)
                    {
                        profiles.Add(profile);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error or handle gracefully
                System.Diagnostics.Debug.WriteLine($"Error loading profiles: {ex.Message}");
            }

            return profiles;
        }

        public static void SaveProfiles(List<Profile> profiles)
        {
            try
            {
                var csvContent = new StringBuilder();
                csvContent.AppendLine(CsvHeader);

                foreach (var profile in profiles)
                {
                    var csvLine = FormatProfileToCsv(profile);
                    csvContent.AppendLine(csvLine);
                }

                File.WriteAllText(CsvFilePath, csvContent.ToString());
            }
            catch (Exception ex)
            {
                // Log error or handle gracefully
                System.Diagnostics.Debug.WriteLine($"Error saving profiles: {ex.Message}");
            }
        }

        public static Profile? ParseProfileFromCsv(string csvLine)
        {
            try
            {
                var parts = SplitCsvLine(csvLine);
                if (parts.Length < 7) return null; // Changed from 8 to 7

                var profile = new Profile
                {
                    Id = parts[0],
                    Name = parts[1],
                    Description = parts[2],
                    Icon = parts[3], // Changed from Color to Icon
                    StartTime = parts[4],
                    EndTime = parts[5],
                    Tasks = string.IsNullOrEmpty(parts[6]) ? new List<string>() : parts[6].Split(';').ToList(), // Changed from | to ;
                    CreatedDate = parts.Length > 7 && DateTime.TryParse(parts[7], out var date) ? date : DateTime.Now // Handle optional CreatedDate
                };

                return profile;
            }
            catch
            {
                return null;
            }
        }

        public static string FormatProfileToCsv(Profile profile)
        {
            var tasks = profile.Tasks.Any() ? string.Join(";", profile.Tasks) : ""; // Changed from | to ;
            return $"{EscapeCsvField(profile.Id)},{EscapeCsvField(profile.Name)},{EscapeCsvField(profile.Description)},{EscapeCsvField(profile.Icon)},{EscapeCsvField(profile.StartTime)},{EscapeCsvField(profile.EndTime)},{EscapeCsvField(tasks)}"; // Changed Color to Icon
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            
            // Escape quotes and wrap in quotes if contains comma, newline, or quote
            if (field.Contains(",") || field.Contains("\n") || field.Contains("\""))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            
            return field;
        }

        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        current.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        // Toggle quote state
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // Field separator
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            // Add the last field
            result.Add(current.ToString());

            return result.ToArray();
        }
    }
}
