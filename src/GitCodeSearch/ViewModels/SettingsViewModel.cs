using GitCodeSearch.Model;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

#pragma warning disable CA1416 // This call site is reachable on all platforms. '{0}' is only supported on: 'Windows' 7.0 and later.

namespace GitCodeSearch.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public enum SettingsSection
    {
        [Description("Git")]
        Git,

        [Description("User interface")]
        UserInterface,

        [Description("Branches")]
        Branches
    }

    public SettingsViewModel()
    {
        AddRepositoriesCommand = new RelayCommand(AddRepositories);
        RemoveRepositoriesCommand = new RelayCommand<IList>(RemoveRepositories, CanRemoveRepositories);
        ActivateAllRepositoriesCommand = new RelayCommand<bool?>(ActivateAllRepositories);
    }

    // Git section
    public RepositoriesViewModel Repositories { get; set; } = new RepositoriesViewModel(Settings.Current.Repositories);
    public bool ShowInactiveRepositoriesInSearchResult { get; set; } = Settings.Current.ShowInactiveRepositoriesInSearchResult;
    public bool WarnOnMissingBranch { get; set; } = Settings.Current.WarnOnMissingBranch;
    public ICommand AddRepositoriesCommand { get; }
    public ICommand RemoveRepositoriesCommand { get; }
    public ICommand ActivateAllRepositoriesCommand { get; }

    // User interface section
    public string PreviewTheme { get; set; } = Settings.Current.PreviewTheme;
    public bool UseTabs { get; set; } = Settings.Current.UseTabs;

    // Branches section
    public string FavouriteBranches { get; set; } = string.Join("\r\n", Settings.Current.FavouriteBranches);
    public string InvalidBranchRegex { get; set; } = Settings.Current.InvalidBranchRegex;

    public static IEnumerable<SettingsSection> SettingsSections => Enum.GetValues<SettingsSection>();

    internal void ApplySettings()
    {
        Settings.Current.Repositories = Repositories;
        Settings.Current.ShowInactiveRepositoriesInSearchResult = ShowInactiveRepositoriesInSearchResult;
        Settings.Current.WarnOnMissingBranch = WarnOnMissingBranch;
        
        Settings.Current.PreviewTheme = PreviewTheme;
        Settings.Current.UseTabs = UseTabs;

        Settings.Current.FavouriteBranches = FavouriteBranches
            .Split("\r\n")
            .Where(b => !string.IsNullOrEmpty(b))
            .Distinct()
            .Select(b => new Branch(b))
            .ToList();
        Settings.Current.InvalidBranchRegex = InvalidBranchRegex;

        Settings.Save();
    }

    private void AddRepositories()
    {
        CommonOpenFileDialog dialog = new() { IsFolderPicker = true };
        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            if (dialog.FileName != null)
            {
                var gitRepositories = Repositories
                    .Select(g => g.Path.ToLowerInvariant())
                    .ToHashSet();

                List<string> repositories = [];
                FindRepositories(ref repositories, dialog.FileName);
                foreach (string repositoryPath in repositories)
                {
                    if (!gitRepositories.Contains(repositoryPath.ToLowerInvariant()))
                        Repositories.Add(new RepositoryViewModel(repositoryPath));
                }
            }
        }
    }

    private void RemoveRepositories(IList selectedItems)
    {
        foreach (var repository in selectedItems.Cast<RepositoryViewModel>().ToList())
        {
            Repositories.Remove(repository);
        }
    }

    private bool CanRemoveRepositories(IList selectedItems)
    {
        return selectedItems.Count > 0;
    }

    private void ActivateAllRepositories(bool? active)
    {
        foreach (var repository in Repositories)
        {
            repository.Active = active == true;
        }
    }

    private static void FindRepositories(ref List<string> repositories, string filePath)
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
}
