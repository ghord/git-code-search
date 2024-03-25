using System.Windows;
using System.Windows.Controls;

namespace GitCodeSearch.ViewModels
{
    public static class DialogHelper
    {
        public static bool ShowDialog(UserControl view, string title, Window owner, double width = 800, double height = 600)
        {
            var dialogWindow = new Window
            {
                Owner = owner,
                Content = view,
                Title = title,
                Width = width,
                Height = height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            return dialogWindow.ShowDialog() == true;
        }
    }
}
