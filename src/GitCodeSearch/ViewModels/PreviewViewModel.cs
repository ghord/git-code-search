using GitCodeSearch.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCodeSearch.ViewModels
{
    public class PreviewViewModel : ViewModelBase
    {
        public PreviewViewModel(SearchResult searchResult, string content)
        {
            Content = content;
        }

        public string Content { get; } 
    }
}
