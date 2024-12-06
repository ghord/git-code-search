using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GitCodeSearch.Model;

public class Repositories : List<Repository>
{
    public async Task<HashSet<string>> GetAllBranchesAsync()
    {
        Regex? invalidBranchNameRegex = CreateInvalidBranchRegex();

        HashSet<string> branches = [];

        foreach (var repository in this)
        {
            if (!repository.IsActiveRepository())
                continue;

            await foreach (var branch in repository.GetBranchesAsync())
            {
                if (invalidBranchNameRegex?.IsMatch(branch) != true)
                    branches.Add(branch);
            }
        }

        return branches;

        static Regex? CreateInvalidBranchRegex()
        {
            if(string.IsNullOrEmpty(Settings.Current.InvalidBranchRegex))
                return null;

            try
            {
                return new Regex(Settings.Current.InvalidBranchRegex, RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateInvalidBranchRegex: {ex.Message}");
                return null;
            }
        }
    }

    public async Task FetchAllAsync(Action<Repository?> action, CancellationToken cancellationToken)
    {
        action(null);

        foreach (var repository in this)
        {
            action(repository);

            if (!repository.IsActiveRepository())
                continue;

            await repository.FetchAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                break;
        }

        action(null);
    }
}
