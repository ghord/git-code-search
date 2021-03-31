using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GitCodeSearch.Converters
{
    public class EnumConverter : IValueConverter
    {
        public Type? EnumType { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter,
                              System.Globalization.CultureInfo culture)
        {
            if (value is Enum e)
            {
                return System.Convert.ToInt32(e);
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter,
                                  System.Globalization.CultureInfo culture)
        {
            if(EnumType is not null && value is int e)
            {
                return Enum.ToObject(EnumType, e);
            }

            throw new NotSupportedException();
        }
    }
}
