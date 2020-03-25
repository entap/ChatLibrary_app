﻿using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Entap.Chat;
using Entap.Chat.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(DynamicResizedEditor), typeof(DynamicResizedEditorRenderer_Droid))]
namespace Entap.Chat.Android
{
    public class DynamicResizedEditorRenderer_Droid : EditorRenderer
    {
        DynamicResizedEditor _dynamicResizedEditor;
        int _lineCount;
        float _density;
        Window _window;

        public DynamicResizedEditorRenderer_Droid(Context context) : base(context)
        {
            var res = context.Resources;
            var resId1 = res.GetIdentifier("status_bar_height", "dimen", "android");
            if (resId1 > 0)
            {
                var metrics = res.DisplayMetrics;
                _density = metrics.Density;
            }
            else
            {
                DisplayMetrics metrics = new DisplayMetrics();
                _density = metrics.Density;
            }

            _window = ((Activity)context).Window;
            _window.SetSoftInputMode(SoftInput.AdjustResize);

        }

        //protected override void Dispose(bool disposing)
        //{
        //    base.Dispose(disposing);
        //    _window.SetSoftInputMode(SoftInput.AdjustNothing);
        //}

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e); 

            //if (this.Control == null) return;
            if (Control != null && e.NewElement != null)
            {
                _dynamicResizedEditor = e.NewElement as DynamicResizedEditor;
                Control.TextChanged += OnTextChanged; 

                _lineCount = Control.LineCount;

                var border = new GradientDrawable();
                border.SetCornerRadius(_dynamicResizedEditor.CornerRadius * _density); //角丸

                border.SetColor(_dynamicResizedEditor.BackgroundColor.ToAndroid());
                //border.SetStroke(customEditor.BorderWidth, customEditor.BorderColor.ToAndroid()); //線の幅・線の色
                //this.Control.SetBackgroundColor(Android.Graphics.Color.Argb(0, 0, 0, 0));
                this.Control.SetBackground(border);

                // SetColorで背景色指定後BackgroundColorをTransparentにしておかないと角丸が見えなくなる
                _dynamicResizedEditor.BackgroundColor = Color.Transparent;

                _dynamicResizedEditor.Focused += OnFocused;
                _dynamicResizedEditor.Unfocused += OnUnFocused;
                SetOneLineSize();
            }
        }

        private void OnTextChanged(object sender, global::Android.Text.TextChangedEventArgs e)
        {
            var lineCount = Control.LineCount;
            if (lineCount > 1)
            {
                SetMultiLineSize();
            }
            else
            {
                SetOneLineSize();
            }
            if (lineCount != _lineCount)
            {
                _dynamicResizedEditor.Resize(lineCount);
                _lineCount = lineCount;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            // 下記処理ないと文字色変わらなかった
            if (e.PropertyName == DynamicResizedEditor.TextColorProperty.PropertyName)
            {
                Control.SetTextColor(_dynamicResizedEditor.TextColor.ToAndroid());
            }

            //if (e.PropertyName == DynamicResizedEditor.ForcedMinimumHeightProperty.PropertyName)
            //{
            //    var dynamicResizedEditor = Element as DynamicResizedEditor;
            //    if (dynamicResizedEditor.ForcedMinimumHeight)
            //    {
            //        dynamicResizedEditor.HeightRequest = dynamicResizedEditor.MinimumHeightRequest;
            //        _lineCount = 1;
            //    }
            //}
            if (e.PropertyName == DynamicResizedEditor.HeightRequestProperty.PropertyName)
            {
                var dynamicResizedEditor = Element as DynamicResizedEditor;
                if (dynamicResizedEditor.HeightRequest > 0)
                {
                    // エディターが入力状態でない時
                    Control.SetSelection(0);
                }
                else
                {
                    // エディターのカーソルを末尾に
                    Control.SetSelection(Control.Text.Length);
                }
            }
            //if (e.PropertyName == DynamicResizedEditor.IsFocusedProperty.PropertyName)
            //{
            //    if (_dynamicResizedEditor.IsFocused)
            //    {
            //        if (_dynamicResizedEditor.HeightRequest > 0)
            //            _dynamicResizedEditor.HeightRequest = -1;
            //        //// エディターのカーソルを末尾に
            //        Device.BeginInvokeOnMainThread(() =>
            //        {
            //            Control.SetSelection(Control.Text.Length);
            //        });
            //    }
            //}
        }

        void OnFocused(object sender, FocusEventArgs e)
        {
            // エディターのカーソルを末尾に
            Device.BeginInvokeOnMainThread(() =>
            {
                Control.SetSelection(Control.Text.Length);

                if (Control.LineCount <= 1)
                {
                    SetOneLineSize();
                }
                else if (_dynamicResizedEditor.HeightRequest > 0)
                {
                    SetMultiLineSize();
                }
            });
        }

        void OnUnFocused(object sender, FocusEventArgs e)
        {
            SetOneLineSize();
            _lineCount = 1;
        }

        void SetOneLineSize()
        {
            _dynamicResizedEditor.HeightRequest = _dynamicResizedEditor.MinimumHeightRequest;
            Control.SetPadding(12, 20, 12, 20);
        }
        void SetMultiLineSize()
        {
            Control.SetPadding(12, 0, 12, 0);
            _dynamicResizedEditor.HeightRequest = -1;
        }
    }
}