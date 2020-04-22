﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace Entap.Chat
{
    public class VideoThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value as string;
            if (string.IsNullOrEmpty(val))
                return null;
            var img = DependencyService.Get<IVideoService>().GenerateThumbImage(val);
            return img;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}