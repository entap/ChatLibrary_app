﻿using System;
using Entap.Chat.iOS;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(DisplayService_iOS))]
namespace Entap.Chat.iOS
{
    public class DisplayService_iOS : IDisplayService
    {
        public Thickness GetSafeArea()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                var window = new UIWindow(UIScreen.MainScreen.Bounds);
                var safeArea = window.SafeAreaInsets;
                return new Thickness(safeArea.Left, safeArea.Top, safeArea.Right, safeArea.Bottom);
            }
            return new Thickness(0, 0, 0, 0);
        }
    }
}