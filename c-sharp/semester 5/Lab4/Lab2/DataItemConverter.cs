using DataLibrary;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Lab2
{
    public class DataItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataItem dataItem)
            {
                return "(Converter)\n" + dataItem.ToString();
            }
            else if (value is DataCollection datacol)
            {
                return "(Converter)\n" + datacol.ToString();
            }
            else return "Error!\n";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && targetType == typeof(DataItem))
            {
                var parts = str.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3)
                {
                    var name_parts = parts[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var date_parts = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var tuple_parts = parts[2].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    return new DataItem { Name = name_parts[2].Trim(), Date = DateTime.Parse(date_parts[2].Trim()), SITuple = (tuple_parts[3], int.Parse(tuple_parts[5])) };
                }
            }
            return "Error!\n";
        }
    }
}