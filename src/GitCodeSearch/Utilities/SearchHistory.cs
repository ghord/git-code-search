using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GitCodeSearch.Utilities;

public class SearchHistory : List<string>
{
    [JsonIgnore]
    public new int Capacity { get; set; } = 30;

    public new void Add(string item)
    {
        Remove(item);
        Insert(0, item);

        if(Count > Capacity)
        {
            RemoveRange(Capacity, Count - Capacity);
        }
    }
}
