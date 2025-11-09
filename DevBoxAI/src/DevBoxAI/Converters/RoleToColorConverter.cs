using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DevBoxAI.Core.Models;

namespace DevBoxAI.Converters;

public class RoleToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ChatRole role)
        {
            return role switch
            {
                ChatRole.User => new SolidColorBrush(Color.FromRgb(232, 245, 233)), // Light Green
                ChatRole.Assistant => new SolidColorBrush(Color.FromRgb(227, 242, 253)), // Light Blue
                ChatRole.System => new SolidColorBrush(Color.FromRgb(255, 243, 224)), // Light Orange
                _ => new SolidColorBrush(Colors.White)
            };
        }

        return new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
