using System.IO;

namespace SPMWPF;

public static class SettingsStore
{
    private static readonly string SettingsPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "spm_settings.ini");

    public static bool GetDontShowWelcome()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return false;
            var lines = File.ReadAllLines(SettingsPath);
            foreach (var l in lines)
            {
                if (l.StartsWith("DontShowWelcome=", System.StringComparison.OrdinalIgnoreCase))
                {
                    var val = l.Split('=')[1].Trim();
                    return val == "1" || val.Equals("true", System.StringComparison.OrdinalIgnoreCase);
                }
            }
        }
        catch { }
        return false;
    }

    public static void SetDontShowWelcome(bool value)
    {
        try
        {
            File.WriteAllText(SettingsPath, $"DontShowWelcome={(value ? 1 : 0)}\n");
        }
        catch { }
    }
}
