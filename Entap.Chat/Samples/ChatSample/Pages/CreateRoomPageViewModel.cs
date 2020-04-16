﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xamarin.Forms;
namespace ChatSample
{
    public class CreateRoomPageViewModel : BindableBase
    {
        public CreateRoomPageViewModel()
        {
        }
        public List<string> InvitationUserIds = new List<string>();

        private string invitationUsers;
        public string InvitationUsers
        {
            get
            {
                return invitationUsers;
            }
            set
            {
                if (invitationUsers != value)
                {
                    invitationUsers = value;
                    OnPropertyChanged("InvitationUsers");
                }
            }
        }

        private string editorText;
        public string EditorText
        {
            get
            {
                return editorText;
            }
            set
            {
                if (editorText != value)
                {
                    editorText = value;
                    OnPropertyChanged("EditorText");
                }
            }
        }
        
        public Command AddCmd => new Command(() =>
        {
            var page = new ContactAddressePage();
            App.Current.MainPage.Navigation.PushAsync(page);
            var vm = ((ContactAddressePageViewModel)page.BindingContext);
            vm.ContactListPageFlag = false;
            vm.ItemTappedCmd = new Command((obj) =>
            {
                if (!(obj is ItemTappedEventArgs itemTappedEventArgs)) return;
                if (!(itemTappedEventArgs.Item is ContactItem selectItem)) return;

                if (vm.ContactListPageFlag)
                    return;
                if (InvitationUserIds.Contains(selectItem.UserId))
                {
                    App.Current.MainPage.Navigation.PopAsync();
                    return;
                }

                InvitationUsers += selectItem.Name + System.Environment.NewLine;
                InvitationUserIds.Add(selectItem.UserId);
                App.Current.MainPage.Navigation.PopAsync();
            });

        });

        public Command ToolbarCmd => new Command(async() =>
        {
            if (string.IsNullOrEmpty(EditorText))
            {
                await App.Current.MainPage.DisplayAlert("", "ルーム名を入力してください", "閉じる");
                return;
            }
            if (InvitationUserIds.Count < 1)
            {
                await App.Current.MainPage.DisplayAlert("", "招待者を選択してください", "閉じる");
                return;
            }

            // RoomType3:管理者以外の人との1対1のルーム  4:管理者以外の人との複数人のルーム
            var data = new ReqCreateRoomData()
            {
                UserId = UserDataManager.Instance.UserId,
                RoomName = EditorText,
                RoomType = 3,
                InvitationUserIds = InvitationUserIds
            };
            if (data.InvitationUserIds.Count > 1)
                data.RoomType = 4;

            var reqJson = JsonConvert.SerializeObject(data);
            var json = await APIManager.PostAsync(APIManager.GetEntapAPI(APIManager.EntapAPIName.CreateRoom), new ReqCreateRoom { Data = reqJson });
            var respCreateRoom = JsonConvert.DeserializeObject<RespCreateRoom>(json);
            if (respCreateRoom.Status == APIManager.APIStatus.Succeeded)
            {
                await App.Current.MainPage.DisplayAlert("", "ルームを作成しました", "閉じる");
                await App.Current.MainPage.Navigation.PopAsync();
            }
        });
    }
}