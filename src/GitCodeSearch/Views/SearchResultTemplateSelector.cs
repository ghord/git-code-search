using GitCodeSearch.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GitCodeSearch.Views
{
    public class SearchResultTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? FileContentTemplate { get; set; }

        public DataTemplate? CommitMessageTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is FileContentSearchResult)
                return FileContentTemplate;
            else if (item is CommitMessageSearchResult)
                return CommitMessageTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
