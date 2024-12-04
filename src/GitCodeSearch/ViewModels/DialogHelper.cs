using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GitCodeSearch.ViewModels;

public static class DialogHelper
{
    public static bool ShowDialog(UserControl view, string title, double width = 800, double height = 600)
    {
        var dialogWindow = new Window
        {
            Owner = Application.Current.MainWindow,
            Content = view,
            Title = title,
            Width = width,
            Height = height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        return dialogWindow.ShowDialog() == true;
    }

    private static readonly Dictionary<UserControl, Window> windows_ = [];

    public static void ShowWindow(UserControl view, string title, double width = 800, double height = 600)
    {
        if (!windows_.TryGetValue(view, out var window))
        {
            window = new Window
            {
                Owner = Application.Current.MainWindow,
                Content = view,
                Title = title,
                Width = width,
                Height = height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            windows_[view] = window;

            window.Closed += (o, e) =>
            {
                windows_.Remove(view);
            };
        }

        window.Show();
       
    }
}
