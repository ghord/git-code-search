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
        Startup += (o, e) => SettingsManager.LoadSettings();
        Exit += (o, e) => SettingsManager.SaveSettings();
    }
}
