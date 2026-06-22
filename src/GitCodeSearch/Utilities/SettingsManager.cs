using GitCodeSearch.Model;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace GitCodeSearch.Utilities;

internal static class SettingsManager
{
    public static readonly string SettingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gitcodesearch");

    public static void LoadSettings()
    {
        try
        {
            using var stream = File.OpenRead(SettingsFilePath);
            Settings.Current = JsonSerializer.Deserialize<Settings>(stream) ?? new Settings();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Settings.Current = new Settings();
        }
    }

    public static void SaveSettings()
    {
        try
        {
            using var stream = File.Create(SettingsFilePath);
            JsonSerializer.Serialize(stream, Settings.Current);
            stream.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
