using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCodeSearch.Model
{
    public class GitRepository
    {
        public bool Active { get; set; } = true;
        public string Path { get; set; } = string.Empty;
    }
}
