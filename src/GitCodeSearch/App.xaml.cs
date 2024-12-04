using GitCodeSearch.Model;
using System.Windows;

namespace GitCodeSearch;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        Startup += (o, e) => Settings.Load();
        Exit += (o, e) => Settings.Save();
    }
}
