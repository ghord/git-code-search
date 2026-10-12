using GitCodeSearch.Mcp;
using GitCodeSearch.Model;
using GitCodeSearch.Utilities;
using System.Windows;

namespace GitCodeSearch;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        Startup += async (o, e) =>
        {
            SettingsManager.LoadSettings();
            await McpServerHost.TryApplyAsync(Settings.Current.McpEnabled, Settings.Current.McpPort);
        };
        Exit += (o, e) =>
        {
            SettingsManager.SaveSettings();
            McpServerHost.StopAsync().GetAwaiter().GetResult();
        };
    }
}
