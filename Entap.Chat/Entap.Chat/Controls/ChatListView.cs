﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Entap.Chat
{
    [Preserve(AllMembers = true)]
    public class ChatListView : ListView
    {
        const int RemainingItemsThreshold = 5;

        ObservableCollection<MessageBase> _messages;
        public ChatListView()
        {
            Init();
        }

        void Init()
        {
            //Scrolled += OnScrolled;
            ItemAppearing += OnItemAppearing;
            //ItemDisappearing += OnItemDisappearing;

            HasUnevenRows = true;
            Task.Run(async() =>
            {
                var messages = await Settings.Current.Messaging.GetMessagesAsync(-1, 20);
                var last = messages?.Last();
                if (last == null) return;
                //// ToDo : 2回目以降に表示時にスクロールが無効
                
                _messages = new ObservableCollection<MessageBase>(messages);
                Device.BeginInvokeOnMainThread(async () =>
                {
                    ItemsSource = _messages;
                    ScrollTo(last, ScrollToPosition.End, false);
                    
                });
            });
        }

        

        void LoadMessages(int messageId)
        {
            Task.Run(async () =>
            {
                var messages = await Settings.Current.Messaging.GetMessagesAsync(messageId, 20);
                //_messages.InsertRange(0, messages);
                foreach (var message in messages.Reverse())
                {
                    _messages.Insert(0, message);
                }
            });
        }

        bool _isRunningGetNewMessage = false;
        void LoadNewMessages(int messageId)
        {
            Task.Run(async () =>
            {
                var messages = await Settings.Current.Messaging.GetNewMessagesAsync(messageId, 20);
                _messages.AddRange(messages);
                _isRunningGetNewMessage = false;
            });
        }

        int _lastAppearingIndex;
        void OnItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnItemAppearing  index :" + e.ItemIndex);
            ScrollDirection direction;
            if (e.ItemIndex < _lastAppearingIndex)
                direction = ScrollDirection.Up;
            else if (e.ItemIndex > _lastAppearingIndex)
                direction = ScrollDirection.Down;
            else
                direction = ScrollDirection.None;

            switch (direction)
            {
                case ScrollDirection.Up:
                    if (_lastAppearingIndex > RemainingItemsThreshold &&
                        e.ItemIndex <= RemainingItemsThreshold)
                    {
                        System.Diagnostics.Debug.WriteLine("Reached Up: " + e.ItemIndex + "  " + _lastAppearingIndex);
                        var first = _messages.First();
                        LoadMessages(first.Id - 1);
                    }
                    break;
                case ScrollDirection.Down:
                    if (_isRunningGetNewMessage) break;

                    var thresholdIndex = _messages.Count - 1 - RemainingItemsThreshold;
                    if (_lastAppearingIndex < thresholdIndex &&
                        e.ItemIndex >= thresholdIndex)
                    {
                        _isRunningGetNewMessage = true;

                        System.Diagnostics.Debug.WriteLine("Reached Up: " + e.ItemIndex + "  " + _lastAppearingIndex);
                        var last = _messages.Last();
                        LoadNewMessages(last.Id + 1);
                    }
                    break;
            }

            _lastAppearingIndex = e.ItemIndex;
        }

        void OnItemDisappearing(object sender, ItemVisibilityEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnItemDisappearing  index :" + e.ItemIndex);
        }

        double _scrollY;
        void OnScrolled(object sender, ScrolledEventArgs e)
        {
            ScrollDirection direction;
            if (e.ScrollY < _scrollY)
                direction = ScrollDirection.Up;
            else if (e.ScrollY > _scrollY)
                direction = ScrollDirection.Down;
            else
                direction = ScrollDirection.None;

            System.Diagnostics.Debug.WriteLine($"direction : {direction}  x : {e.ScrollX}  y : {e.ScrollY}");

            switch (direction)
            {
                case ScrollDirection.Up:
                    if (_scrollY > RemainingItemsThreshold &&
                        e.ScrollY <= RemainingItemsThreshold)
                    {
                        System.Diagnostics.Debug.WriteLine("Reached Up: " + e.ScrollY + "  " + _scrollY);
                        var first = _messages.First();
                        LoadMessages(first.Id - 1);
                    }

                    break;
                //case ScrollDirection.Down:
                //    if (_lastVisibleItemIndex == e.LastVisibleItemIndex) break;

                //    var thresholdIndex = _messages.Count - 1 - RemainingItemsThreshold;
                //    if (_lastVisibleItemIndex < thresholdIndex &&
                //        e.LastVisibleItemIndex >= thresholdIndex)
                //    {
                //        System.Diagnostics.Debug.WriteLine("Reached Down: " + e.LastVisibleItemIndex + " " + _lastVisibleItemIndex);
                //        var last = _messages.Last();
                //        LoadNewMessages(last.Id + 1);
                //    }
                //    _lastVisibleItemIndex = e.LastVisibleItemIndex;
                //    break;
            }
            _scrollY = e.ScrollY;
        }

        //int _firstVisibleItemIndex = 0;
        //int _lastVisibleItemIndex = 0;
        //void OnScrolled(object sender, ItemsViewScrolledEventArgs e)
        //{

        //    ScrollDirection direction;
        //    if (e.VerticalDelta < 0)
        //        direction = ScrollDirection.Up;
        //    else if (e.VerticalDelta > 0)
        //        direction = ScrollDirection.Down;
        //    else
        //        direction = ScrollDirection.None;

        //    switch (direction)
        //    {
        //        case ScrollDirection.Up:
        //            if (_firstVisibleItemIndex == e.FirstVisibleItemIndex) break;

        //            if (_firstVisibleItemIndex > RemainingItemsThreshold &&
        //                e.FirstVisibleItemIndex <= RemainingItemsThreshold)
        //            {
        //                System.Diagnostics.Debug.WriteLine("Reached Up: " + e.FirstVisibleItemIndex + "  " + _firstVisibleItemIndex);
        //                var first = _messages.First();
        //                LoadMessages(first.Id - 1);
        //            }
        //            _firstVisibleItemIndex = e.FirstVisibleItemIndex;

        //            break;
        //        case ScrollDirection.Down:
        //            if (_lastVisibleItemIndex == e.LastVisibleItemIndex) break;

        //            var thresholdIndex = _messages.Count - 1 - RemainingItemsThreshold;
        //            if (_lastVisibleItemIndex < thresholdIndex &&
        //                e.LastVisibleItemIndex >= thresholdIndex)
        //            {
        //                System.Diagnostics.Debug.WriteLine("Reached Down: " + e.LastVisibleItemIndex + " " + _lastVisibleItemIndex);
        //                var last = _messages.Last();
        //                LoadNewMessages(last.Id + 1);
        //            }
        //            _lastVisibleItemIndex = e.LastVisibleItemIndex;
        //            break;
        //    }
        //}

        enum ScrollDirection
        {
            Up,
            Down,
            None
        }
    }
}