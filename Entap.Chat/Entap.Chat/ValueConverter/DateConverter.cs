﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace Entap.Chat
{
    public class DateConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = (DateTime)value;
            if (dateTime.ToString("yyyy/MM/dd") == DateTime.Now.ToString("yyyy/MM/dd"))
                return Settings.Current.TodayText;
            return dateTime.ToString(Settings.Current.DateFormat);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
