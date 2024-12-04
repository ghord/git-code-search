using GitCodeSearch.Converters;
using System.Linq;
using System.Text.Json.Serialization;

namespace GitCodeSearch.Model;

[JsonConverter(typeof(BranchConverter))]
public class Branch(string branch)
{
    public static readonly Branch Empty = new(string.Empty);

    public bool IsFavourite { get; set; }

    public string Name { get; } = GetBranch(branch);
    public string Remote { get; } = GetRemote(branch);

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Remote))
        {
            if (string.IsNullOrEmpty(Name))
            {
                return "(local)";
            }

            return Name;
        }

        return $"{Remote}/{Name}";
    }

    public static implicit operator string(Branch branch) => branch.ToString();

    private static string GetRemote(string branch) => branch.Split("/").First();
    private static string GetBranch(string branch) => branch.Split("/").Last();


}
