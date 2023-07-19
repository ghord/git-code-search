using GitCodeSearch.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

#pragma warning disable CA1416 // This call site is reachable on all platforms. '{0}' is only supported on: 'Windows' 7.0 and later.

namespace GitCodeSearch.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).DialogResult = true;
        }

        private SettingsViewModel settings_ => (SettingsViewModel)DataContext;

        private void FindRepositories_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new() { IsFolderPicker = true };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (dialog.FileName != null)
                {
                    var gitRepositories = settings_.GitRepositories
                        .Select(g => g.Path.ToLowerInvariant())
                        .ToHashSet();

                    List<string> repositories = new();
                    FindRepositories(ref repositories, dialog.FileName);
                    foreach(string repository in repositories)
                    {
                        if(!gitRepositories.Contains(repository.ToLowerInvariant()))
                            settings_.GitRepositories.Add(new GitRepositoryViewModel { Path = repository });
                    }
                }
            }
        }

        private void FindRepositories(ref List<string> repositories, string filePath)
        {
            if (Directory.Exists(Path.Combine(filePath, ".git")))
            {
                repositories.Add(filePath);
            }
            else
            {
                foreach (var directory in Directory.GetDirectories(filePath))
                {
                    FindRepositories(ref repositories, directory);
                }
            }
        }

        private void RemoveRepositories_Click(object sender, RoutedEventArgs e)
        {
            var gitRepositories = settings_.GitRepositories;
            foreach (var item in RepositoriesDataGrid.SelectedItems.Cast<GitRepositoryViewModel>().ToList())
            {
                if(item is GitRepositoryViewModel gitRepository)
                    gitRepositories.Remove(gitRepository);
            }
            settings_.GitRepositories = gitRepositories;
        }

        private void RepositoriesDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == nameof(GitRepositoryViewModel.Active))
            {
                FrameworkElementFactory cellCheckboxFactory = CreateCellCheckboxFactory(e.PropertyName);
                FrameworkElementFactory headerCheckboxFactory = CreateHeaderCheckboxFactory(e.PropertyName);

                e.Column = new DataGridTemplateColumn()
                {
                    Header = e.Column.Header,
                    CellTemplate = new DataTemplate { VisualTree = cellCheckboxFactory },
                    HeaderTemplate = new DataTemplate { VisualTree = headerCheckboxFactory }
                };

            }
        }

        private static FrameworkElementFactory CreateCellCheckboxFactory(string propertyName)
        {
            var binding = new Binding(propertyName)
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            var checkboxFactory = new FrameworkElementFactory(typeof(CheckBox));

            checkboxFactory.SetBinding(ToggleButton.IsCheckedProperty, binding);
            checkboxFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            checkboxFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);

            return checkboxFactory;
        }

        private FrameworkElementFactory CreateHeaderCheckboxFactory(string propertyName)
        {
            var binding = new Binding(propertyName)
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            var checkboxFactory = new FrameworkElementFactory(typeof(CheckBox));
            checkboxFactory.AddHandler(CheckBox.ClickEvent, new RoutedEventHandler(CheckAllCheckbox_Click));
            checkboxFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            checkboxFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            checkboxFactory.SetValue(CheckBox.ContentProperty, propertyName);

            return checkboxFactory;
        }

        private void CheckAllCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                foreach (var gitRepository in settings_.GitRepositories)
                {
                    gitRepository.Active = checkBox.IsChecked == true;
                    
                }
            }
        }


    }
}
