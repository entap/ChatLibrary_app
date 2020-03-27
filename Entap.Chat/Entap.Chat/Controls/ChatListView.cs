﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Entap.Chat
{
    /*
    [Preserve(AllMembers = true)]
    public class ChatListView : ListView
    {
        const int RemainingItemsThreshold = 5;

        ObservableCollection<MessageBase> _messages;
        //public ChatListView() : base(ListViewCachingStrategy.RecycleElement)
        public ChatListView()
        {
            Init();
        }

        void Init()
        {
            //Scrolled += OnScrolled;
            ItemAppearing += OnItemAppearing;
            //ItemDisappearing += OnItemDisappearing;
            SelectionMode = ListViewSelectionMode.None;
            SeparatorVisibility = SeparatorVisibility.None;
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


        bool _isRunningGetMessage = false;
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
                _isRunningGetMessage = false;
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
            if (_isRunningGetMessage) return;
            if (_isRunningGetNewMessage) return;
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
                    if (_isRunningGetMessage) break;

                    if (_lastAppearingIndex > RemainingItemsThreshold &&
                        e.ItemIndex <= RemainingItemsThreshold)
                    {
                        _isRunningGetMessage = true;
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

                        System.Diagnostics.Debug.WriteLine("Reached Down: " + e.ItemIndex + "  " + _lastAppearingIndex);
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

        public bool AddMessage(MessageBase msg)
        {
            _messages.Add(msg);
            Task.Run(async() =>
            {
                await Task.Delay(500);
                Device.BeginInvokeOnMainThread(() => ScrollTo(msg, ScrollToPosition.End, false));
            });
            return true;
        }

        int _firstVisibleItemIndex = 0;
        int _lastVisibleItemIndex = 0;
        void OnScrolled(object sender, ItemsViewScrolledEventArgs e)
        {

            ScrollDirection direction;
            if (e.VerticalDelta < 0)
                direction = ScrollDirection.Up;
            else if (e.VerticalDelta > 0)
                direction = ScrollDirection.Down;
            else
                direction = ScrollDirection.None;

            switch (direction)
            {
                case ScrollDirection.Up:
                    if (_firstVisibleItemIndex == e.FirstVisibleItemIndex) break;

                    if (_firstVisibleItemIndex > RemainingItemsThreshold &&
                        e.FirstVisibleItemIndex <= RemainingItemsThreshold)
                    {
                        System.Diagnostics.Debug.WriteLine("Reached Up: " + e.FirstVisibleItemIndex + "  " + _firstVisibleItemIndex);
                        var first = _messages.First();
                        LoadMessages(first.Id - 1);
                    }
                    _firstVisibleItemIndex = e.FirstVisibleItemIndex;

                    break;
                case ScrollDirection.Down:
                    if (_lastVisibleItemIndex == e.LastVisibleItemIndex) break;

                    var thresholdIndex = _messages.Count - 1 - RemainingItemsThreshold;
                    if (_lastVisibleItemIndex < thresholdIndex &&
                        e.LastVisibleItemIndex >= thresholdIndex)
                    {
                        System.Diagnostics.Debug.WriteLine("Reached Down: " + e.LastVisibleItemIndex + " " + _lastVisibleItemIndex);
                        var last = _messages.Last();
                        LoadNewMessages(last.Id + 1);
                    }
                    _lastVisibleItemIndex = e.LastVisibleItemIndex;
                    break;
            }
        }

        enum ScrollDirection
        {
            Up,
            Down,
            None
        }
    }
    */


    public class ChatListView : ListView
    {
        const int RemainingItemsThreshold = 7;

        ObservableCollection<MessageBase> _messages;
        public ChatListView() : base(ListViewCachingStrategy.RecycleElement)
        {
            Init();
        }

        void Init()
        {
            HasUnevenRows = true;
            SeparatorVisibility = SeparatorVisibility.None;
            Scrolled += OnScrolled; ;
            ItemAppearing += OnItemAppearing;
            ItemDisappearing += OnItemDisappearing;

            Task.Run(async () =>
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

        bool IsRunningGetOldMessage = false;
        void LoadMessages(int messageId)
        {
            Task.Run(async () =>
            {
                var messages = await Settings.Current.Messaging.GetMessagesAsync(messageId, 20);
                //if (Device.RuntimePlatform == Device.Android)
                //{
                //    Device.BeginInvokeOnMainThread(() =>
                //    {
                //        IsEnabled = false;
                //        foreach (var msg in messages.Reverse())
                //        {
                //            _messages.Insert(0, msg);
                //        }
                //        ScrollTo(firstVisibleItem, ScrollToPosition.Start, false);
                //        IsEnabled = true;
                //        //firstVisibleItemIndexを一度ありえない値にしておかないとどんどん前のデータの読み込みが行われる
                //        firstVisibleItemIndex = -1;
                //        IsRunningGetOldMessage = false;
                //    });
                //}
                //else if (Device.RuntimePlatform == Device.iOS)
                //{
                //    Device.BeginInvokeOnMainThread(() =>
                //    {
                //        IsEnabled = false;
                //        foreach (var msg in messages.Reverse())
                //        {
                //            _messages.Insert(0, msg);
                //        }
                //        //ScrollTo(firstVisibleItem, ScrollToPosition.Start, false);
                //        IsEnabled = true;
                //        // firstVisibleItemIndexを一度ありえない値にしておかないとどんどん前のデータの読み込みが行われる
                //        firstVisibleItemIndex = -1;
                //        IsRunningGetOldMessage = false;
                //    });
                //}

                Device.BeginInvokeOnMainThread(() =>
                {
                    IsEnabled = false;
                    foreach (var msg in messages.Reverse())
                    {
                        _messages.Insert(0, msg);
                    }
                    if (Device.RuntimePlatform == Device.Android)
                        ScrollTo(firstVisibleItem, ScrollToPosition.Start, false);
                    IsEnabled = true;
                    // firstVisibleItemIndexを一度ありえない値にしておかないとどんどん前のデータの読み込みが行われる
                    firstVisibleItemIndex = -1;
                    IsRunningGetOldMessage = false;
                });
            });
        }

        bool IsRunningGetNewMessage = false;
        void LoadNewMessages(int messageId)
        {
            Task.Run(async () =>
            {
                var messages = await Settings.Current.Messaging.GetNewMessagesAsync(messageId, 20);
                Device.BeginInvokeOnMainThread(() =>
                {
                    _messages.AddRange(messages);
                    IsRunningGetNewMessage = false;
                });
            });
        }

        int firstVisibleItemIndex;
        object firstVisibleItem;
        int lastVisibleItemIndex;
        object lastVisibleItem;

        ScrollDirection chatScrollDirection;

        private void OnItemDisappearing(object sender, ItemVisibilityEventArgs e)
        {
            if (chatScrollDirection == ScrollDirection.Down)
            {
                lastVisibleItemIndex = e.ItemIndex;
                lastVisibleItem = e.Item;
            }
            else if (chatScrollDirection == ScrollDirection.Up)
            {
                firstVisibleItemIndex = e.ItemIndex;
                firstVisibleItem = e.Item;
            }
        }

        private void OnItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if (chatScrollDirection == ScrollDirection.Up)
            {
                firstVisibleItemIndex = e.ItemIndex;
                firstVisibleItem = e.Item;
            }
            else if (chatScrollDirection == ScrollDirection.Down)
            {
                lastVisibleItemIndex = e.ItemIndex;
                lastVisibleItem = e.Item;
            }
        }

        double lastScrollY = 0;
        private void OnScrolled(object sender, ScrolledEventArgs e)
        {
            if (lastScrollY > e.ScrollY)
            {
                if (Device.RuntimePlatform == Device.Android)
                    chatScrollDirection = ScrollDirection.Down;
                else if (Device.RuntimePlatform == Device.iOS)
                    chatScrollDirection = ScrollDirection.Up;
            }   
            else if (lastScrollY < e.ScrollY)
            {
                if (Device.RuntimePlatform == Device.Android)
                    chatScrollDirection = ScrollDirection.Up;
                else if (Device.RuntimePlatform == Device.iOS)
                    chatScrollDirection = ScrollDirection.Down;
            }
            else
            {
                chatScrollDirection = ScrollDirection.None;
                return;
            }

            if (
                firstVisibleItemIndex <= RemainingItemsThreshold &&
                firstVisibleItemIndex > 0 &&
                chatScrollDirection == ScrollDirection.Up &&
                !IsRunningGetOldMessage
                )
            {
                IsRunningGetOldMessage = true;
                var first = _messages.First();
                LoadMessages(first.Id - 1);
            }
            else if (
                lastVisibleItemIndex >= _messages.Count - 1 - RemainingItemsThreshold &&
                lastVisibleItemIndex <= _messages.Count - 1 &&
                chatScrollDirection == ScrollDirection.Down &&
                !IsRunningGetNewMessage
                )
            {
                IsRunningGetNewMessage = true;
                var last = _messages.Last();
                LoadNewMessages(last.Id + 1);
            }

            lastScrollY = e.ScrollY;
        }

        public bool AddMessage(MessageBase msg)
        {
            _messages.Add(msg);
            ScrollTo(msg, ScrollToPosition.End, true);
            return true;
        }

        /// <summary>
        /// データの読み込みを始める未表示データの値
        /// </summary>
        //public static readonly BindableProperty RemainingItemsThresholdProperty =
        //    BindableProperty.Create(nameof(RemainingItemsThreshold), typeof(int), typeof(ChatListView_Seki), null,
        //        propertyChanged: (bindable, oldValue, newValue) =>
        //                            ((ChatListView_Seki)bindable).RemainingItemsThreshold = (int)newValue);

        //public int RemainingItemsThreshold
        //{
        //    get { return (int)GetValue(RemainingItemsThresholdProperty); }
        //    set { SetValue(RemainingItemsThresholdProperty, value); }
        //}

        enum ScrollDirection
        {
            Up,
            Down,
            None
        }
    }
}
