using GitCodeSearch.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace GitCodeSearch.Views;

/// <summary>
/// Interaction logic for GitSettingsView.xaml
/// </summary>
public partial class GitSettingsView : UserControl
{
    public GitSettingsView()
    {
        InitializeComponent();
    }

    private void RepositoriesDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (e.PropertyName == nameof(RepositoryViewModel.Active))
        {
            FrameworkElementFactory cellCheckboxFactory = CreateCellCheckboxFactory(e.PropertyName);
            FrameworkElementFactory headerCheckboxFactory = CreateHeaderCheckboxFactory(e.PropertyName);

            e.Column = new DataGridTemplateColumn()
            {
                Header = e.Column.Header,
                CellTemplate = new DataTemplate { VisualTree = cellCheckboxFactory },
                HeaderTemplate = new DataTemplate { VisualTree = headerCheckboxFactory }
            };

        }
    }

    private static FrameworkElementFactory CreateCellCheckboxFactory(string propertyName)
    {
        var checkboxFactory = new FrameworkElementFactory(typeof(CheckBox));

        checkboxFactory.SetBinding(ToggleButton.IsCheckedProperty, new Binding(propertyName) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        checkboxFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
        checkboxFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);

        return checkboxFactory;
    }

    private FrameworkElementFactory CreateHeaderCheckboxFactory(string propertyName)
    {
        var checkboxFactory = new FrameworkElementFactory(typeof(CheckBox));

        checkboxFactory.SetBinding(DataContextProperty, new Binding(nameof(DataContext)) { ElementName = RepositoriesDataGrid.Name });
        checkboxFactory.SetBinding(ButtonBase.CommandProperty, new Binding(nameof(SettingsViewModel.ActivateAllRepositoriesCommand)));
        checkboxFactory.SetBinding(ButtonBase.CommandParameterProperty, new Binding(nameof(CheckBox.IsChecked)) { RelativeSource = RelativeSource.Self });
        checkboxFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
        checkboxFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
        checkboxFactory.SetValue(ContentProperty, propertyName);

        return checkboxFactory;
    }
}
