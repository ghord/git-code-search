using System.Text.Json.Serialization;

namespace GitCodeSearch.Model
{
    public class GitRepository
    {
        public bool Active { get; set; } = true;

        private string path = string.Empty;
        public string Path 
        {
            get => path;
            set
            {
                path = value;
                Name = System.IO.Path.GetFileName(value);
            }
        }

        [JsonIgnore]
        public string Name { get; private set; } = string.Empty;
    }
}
