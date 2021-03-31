using GitCodeSearch.Model;
using GitCodeSearch.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitCodeSearch.Views
{
    /// <summary>
    /// Interaction logic for PreviewView.xaml
    /// </summary>
    public partial class PreviewView : UserControl
    {
        public PreviewView()
        {
            InitializeComponent();
        }

        public void ScrollToSearchResult(FileContentSearchResult searchResult)
        {
            ContentTextBox.ScrollToLine(searchResult.Line);
            var index = LineCounter.GetCharacterIndex(ContentTextBox.Text, searchResult.Line, searchResult.Column);

            var contentLength = ContentTextBox.Text.Length;

            ContentTextBox.Select(index, searchResult.Query.Expression.Length);
            ContentTextBox.Focus();
        }
    }
}
