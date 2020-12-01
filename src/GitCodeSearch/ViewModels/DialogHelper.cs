using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GitCodeSearch.ViewModels
{
    public static class DialogHelper
    {
        public static bool ShowDialog(UserControl view, string title, Window owner)
        {
            var dialogWindow = new Window();

            dialogWindow.Owner = owner;
            dialogWindow.Content = view;
            dialogWindow.Title = title;
            dialogWindow.Width = 800;
            dialogWindow.Height = 600;
            dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            return dialogWindow.ShowDialog() == true;
        }
    }
}
