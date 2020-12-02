using GitCodeSearch.Model;
using GitCodeSearch.ViewModels;
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
using System.Windows.Threading;

namespace GitCodeSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadDataContextAsync().WaitAndDispatch();
        }

        public async Task LoadDataContextAsync()
        {
            var settings = await Settings.LoadAsync(Settings.DefaultPath) ?? new Settings();
            var viewModel = new MainViewModel(this, settings);
            await viewModel.UpdateBranchesAsync();
            this.DataContext = viewModel;
        }

        private  void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        { 
            var lbi = (ListBoxItem)sender;
            var viewModel = (MainViewModel)DataContext;
            viewModel.ShowPreviewCommand.Execute(lbi.DataContext);
        }
    }
}
