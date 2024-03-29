﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Entap.Chat;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ChatSample
{
    public class RoomListPageViewMode : BindableBase
    {
        Dictionary<int, List<ChatMemberBase>> memberData = new Dictionary<int, List<ChatMemberBase>>();
        public RoomListPageViewMode()
        {
            ItemsSource = new ObservableCollection<Room>();

            GetData();
        }

        void GetData()
        {
            Task.Run(async () =>
            {
                var json = await APIManager.PostAsync(APIManager.GetEntapAPI(APIManager.EntapAPIName.GetRoomList), new ReqGetRoomList());
                var respGetRooms = JsonConvert.DeserializeObject<RespGetRoomList>(json);
                if (respGetRooms.Status == APIManager.APIStatus.Succeeded)
                {
                    var chatService = new ChatService();
                    if (respGetRooms.Data.Rooms.Count < 1)
                    {
                        //サービス管理者とのルーム作成
                        var data = new ReqCreateRoomData()
                        {
                            UserId = UserDataManager.Instance.UserId,
                            RoomType = RoomTypes.AdminDirect
                        };
                        var reqJson = JsonConvert.SerializeObject(data);
                        json = await APIManager.PostAsync(APIManager.GetEntapAPI(APIManager.EntapAPIName.CreateRoom), new ReqCreateRoom { Data = reqJson });
                        var respCreateRoom = JsonConvert.DeserializeObject<RespCreateRoom>(json);
                        if (respCreateRoom.Status == APIManager.APIStatus.Succeeded)
                        {
                            var members = await chatService.GetRoomMembers(respCreateRoom.Data.RoomId);
                            if (memberData.ContainsKey(respCreateRoom.Data.RoomId))
                                memberData.Remove(respCreateRoom.Data.RoomId);
                            memberData.Add(respCreateRoom.Data.RoomId, members);
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                ItemsSource.Add(respCreateRoom.Data);
                            });
                        }
                    }
                    else
                    {
                        foreach (var room in respGetRooms.Data.Rooms)
                        {
                            var members = await chatService.GetRoomMembers(room.RoomId);
                            if (memberData.ContainsKey(room.RoomId))
                                memberData.Remove(room.RoomId);
                            memberData.Add(room.RoomId, members);
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                ItemsSource.Add(room);
                            });
                        }
                    }
                }
            });
        }

        private ObservableCollection<Room> itemsSource;
        public ObservableCollection<Room> ItemsSource
        {
            get
            {
                return itemsSource;
            }
            set
            {
                if (itemsSource != value)
                {
                    itemsSource = value;
                    OnPropertyChanged("ItemsSource");
                }
            }
        }
        
        public Command MemberAddCmd => new Command(async (roomId) =>
        {
            var page = new CreateRoomPage(2, (int)roomId);
            await App.Current.MainPage.Navigation.PushAsync(page);
        });

        public Command LeaveCmd => new Command(async(roomId) =>
        {
            var data = new ReqLeaveRoom { RoomId = (int)roomId };
            var json = await APIManager.PostAsync(APIManager.GetEntapAPI(APIManager.EntapAPIName.LeaveRoom), data);
            var resp = JsonConvert.DeserializeObject<ResponseBase>(json);
            if (resp.Status == APIManager.APIStatus.Succeeded)
            {
                await App.Current.MainPage.DisplayAlert("", "退出しました", "閉じる");
                ItemsSource = new ObservableCollection<Room>();
                GetData();
            }
        });

        public Command ItemTappedCmd => new Command((obj) =>
        {
            if (!(obj is ItemTappedEventArgs itemTappedEventArgs)) return;
            if (!(itemTappedEventArgs.Item is Room model)) return;

            var page = new ChatPage(model);
            App.Current.MainPage.Navigation.PushAsync(page);
        });
    }
}
