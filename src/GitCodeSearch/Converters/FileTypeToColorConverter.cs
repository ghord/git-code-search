using GitCodeSearch.Model;
using System;
using System.Windows.Data;

namespace GitCodeSearch.Converters;

public class FileTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is FileType fileType)
        {
            return fileType switch
            {
                FileType.Cs => System.Windows.Media.Brushes.DodgerBlue,
                FileType.Csproj => System.Windows.Media.Brushes.Green,
                FileType.Xml => System.Windows.Media.Brushes.LightCoral,
                FileType.Json => System.Windows.Media.Brushes.Peru,
                FileType.Yaml => System.Windows.Media.Brushes.MediumOrchid,
                FileType.Js => System.Windows.Media.Brushes.Crimson,
                FileType.Ts => System.Windows.Media.Brushes.LightSkyBlue,
                FileType.Css => System.Windows.Media.Brushes.Gold,
                FileType.Html => System.Windows.Media.Brushes.Teal,
                FileType.Sql => System.Windows.Media.Brushes.YellowGreen,
                _ => System.Windows.Media.Brushes.Gray,
            };
        }

        return System.Windows.Media.Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
