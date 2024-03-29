﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Entap.Chat;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ChatSample
{
    public class ChatControlService : IChatControlService
    {
        /// <summary>
        /// 写真撮影
        /// </summary>
        /// <returns></returns>
        public async Task<string> TakePicture()
        {
            var mg = new MediaPluginManager();
            var path = await mg.TakePhotoAsync();
            if (path is null)
                return await Task.FromResult<string>("");
            return await Task.FromResult<string>(path);
        }

        /// <summary>
        /// 動画撮影
        /// </summary>
        /// <returns></returns>
        public async Task<string> TakeVideo()
        {
            var mg = new MediaPluginManager();
            var path = await mg.TakeVideoAsync();
            if (path is null)
                return await Task.FromResult<string>("");
            return await Task.FromResult<string>(path);
        }

        /// <summary>
        /// 動画撮影
        /// </summary>
        /// <returns></returns>
        public async Task<string> SelectVideo()
        {
            var mg = new MediaPluginManager();
            var path = await mg.PickVideoAsync();
            if (path is null)
                return await Task.FromResult<string>("");
            return await Task.FromResult<string>(path);
        }

        /// <summary>
        /// ライブラリからの画像選択
        /// </summary>
        /// <returns></returns>
        public async Task<string> SelectImage()
        {
            var mg = new MediaPluginManager();
            var paths = await mg.PickPhotoAsyncGetPathAndAlbumPath();
            if (paths is null)
                return await Task.FromResult<string>("");
            byte[] bytes = null;
            string extension = "";
            string sendImgUrl = "";
            if (!string.IsNullOrEmpty(paths[0]))
            {
                sendImgUrl = paths[0];
                bytes = FileManager.ReadBytes(sendImgUrl);
                if ((bytes == null || bytes.Length < 1) && paths.Count > 1)
                {
                    sendImgUrl = paths[1];
                    bytes = FileManager.ReadBytes(sendImgUrl);
                    extension = System.IO.Path.GetExtension(sendImgUrl);
                }
            }

            if (bytes == null || bytes.Length < 1)
            {
                if (bytes != null && bytes.Length < 1)
                    await App.Current.MainPage.DisplayAlert(null, "こちらの画像は送信できません", "閉じる");
                return await Task.FromResult<string>("");
            }

            return await Task.FromResult<string>(sendImgUrl);
        }

        /// <summary>
        /// 画像ファイルの共有
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public async Task ImageShare(string imagePath)
        {
            await MediaManager.ImageShare(imagePath);
        }

        /// <summary>
        /// 動画ファイルの共有
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public async Task VideoShare(string videoPath)
        {
            await MediaManager.VideoShare(videoPath);
        }

        /// <summary>
        /// 画像のプレビューページへ遷移
        /// </summary>
        /// <param name="imageUrl"></param>
        public void MoveImagePreviewPage(string imageUrl)
        {
            App.Current.MainPage.Navigation.PushModalAsync(new ImagePreviewPage(imageUrl));
        }

        /// <summary>
        /// 動画のプレビューページへ遷移
        /// </summary>
        /// <param name="imageUrl"></param>
        public void MoveVideoPreviewPage(string imageUrl)
        {
            App.Current.MainPage.Navigation.PushModalAsync(new VideoPreviewPage(imageUrl));
        }

        /// <summary>
        /// BottomControllerのMenuViewの各メニュー押した際の動作指定
        /// </summary>
        /// <param name="notSendMessageId"></param>
        /// <param name="type"></param>
        /// <param name="roomId"></param>
        /// <param name="chatListView"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MessageBase>> BottomControllerMenuExecute(int notSendMessageId, int type, int roomId, ChatListView chatListView)
        {
            string photoStr = "写真";
            string VideoStr = "動画";
            string cancelStr = "キャンセル";
            if (type == (int)BottomControllerMenuType.Camera)
            {
                var selected = await App.Current.MainPage.DisplayActionSheet("選択してください", cancelStr, null, new string[2] { photoStr, VideoStr });
                if (selected == photoStr)
                {
                    var imgPath = await TakePicture();
                    if (string.IsNullOrEmpty(imgPath))
                        return await Task.FromResult<IEnumerable<MessageBase>>(null);
                    byte[] bytes = FileManager.ReadBytes(imgPath);
                    string extension = System.IO.Path.GetExtension(imgPath);
                    string name = Guid.NewGuid().ToString() + extension;
                    if (bytes == null || bytes.Length < 1)
                    {
                        return await Task.FromResult<IEnumerable<MessageBase>>(null);
                    }
                    var copyImgPath = Settings.Current.ChatService.GetSendImageSaveFolderPath() + Guid.NewGuid() + extension;
                    if (!FileManager.FileCopy(imgPath, copyImgPath))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Application.Current.MainPage.DisplayAlert("", "画像の取得に失敗しました", "閉じる");
                        });
                        return await Task.FromResult<IEnumerable<MessageBase>>(null);
                    }
                    var msg = new ImageMessage { MessageId = notSendMessageId, MediaUrl = copyImgPath, MessageType = (int)MessageType.Image, SendUserId = Settings.Current.ChatService.GetUserId() };
                    return await Task.FromResult<IEnumerable<MessageBase>>(new List<MessageBase> { msg });
                }
                else if (selected == VideoStr)
                {
                    var videoPath = await TakeVideo();
                    if (string.IsNullOrEmpty(videoPath))
                        return await Task.FromResult<IEnumerable<MessageBase>>(null);
                    byte[] bytes = FileManager.ReadBytes(videoPath);
                    string extension = ".mp4";
                    string name = Guid.NewGuid().ToString() + extension;
                    if (bytes == null || bytes.Length < 1)
                    {
                        return await Task.FromResult<IEnumerable<MessageBase>>(null);
                    }
                    var copyPath = Settings.Current.ChatService.GetSendVideoSaveFolderPath() + Guid.NewGuid() + extension;
                    bool result;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        result = DependencyService.Get<IVideoService>().ConvertMp4(videoPath, copyPath);
                    }
                    else
                    {
                        result = FileManager.FileCopy(videoPath, copyPath);
                    }
                    if (result)
                    {
                        var msg = new VideoMessage { MessageId = notSendMessageId, MediaUrl = copyPath, MessageType = (int)MessageType.Video, SendUserId = Settings.Current.ChatService.GetUserId() };
                        return await Task.FromResult<IEnumerable<MessageBase>>(new List<MessageBase> { msg });
                    }
                    return await Task.FromResult<IEnumerable<MessageBase>>(null);
                    
                }
            }
            else if (type == (int)BottomControllerMenuType.Library)
            {
                var selected = await App.Current.MainPage.DisplayActionSheet("選択してください", cancelStr, null, new string[2] { photoStr, VideoStr });
                if (selected == photoStr)
                {
                    var imgPath = await SelectImage();
                    if (string.IsNullOrEmpty(imgPath))
                        return await Task.FromResult<IEnumerable<MessageBase>>(null);
                    byte[] bytes = FileManager.ReadBytes(imgPath);
                    string extension = System.IO.Path.GetExtension(imgPath);
                    string name = Guid.NewGuid().ToString() + extension;
                    if (bytes == null || bytes.Length < 1)
                    {
                        return await Task.FromResult<IEnumerable<MessageBase>>(null);
                    }
                    var copyImgPath = Settings.Current.ChatService.GetSendImageSaveFolderPath() + Guid.NewGuid() + extension;
                    if (!FileManager.FileCopy(imgPath, copyImgPath))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Application.Current.MainPage.DisplayAlert("", "画像の取得に失敗しました", "閉じる");
                        });
                        return await Task.FromResult<IEnumerable<MessageBase>>(null);
                    }
                    var msg = new ImageMessage { MessageId = notSendMessageId, MediaUrl = copyImgPath, MessageType = (int)MessageType.Image, SendUserId = Settings.Current.ChatService.GetUserId() };
                    return await Task.FromResult<IEnumerable<MessageBase>>(new List<MessageBase> { msg });
                }
                else if (selected == VideoStr)
                {
                    var videoPath = await SelectVideo();
                    if (string.IsNullOrEmpty(videoPath))
                        return await Task.FromResult<IEnumerable<MessageBase>>(null);
                    byte[] bytes = FileManager.ReadBytes(videoPath);
                    string extension = ".mp4";
                    string name = Guid.NewGuid().ToString() + extension;
                    if (bytes == null || bytes.Length < 1)
                    {
                        return await Task.FromResult<IEnumerable<MessageBase>>(null);
                    }
                    var copyPath = Settings.Current.ChatService.GetSendVideoSaveFolderPath() + Guid.NewGuid() + extension;
                    bool result;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        result = DependencyService.Get<IVideoService>().ConvertMp4(videoPath, copyPath);
                    }
                    else
                    {
                        result = FileManager.FileCopy(videoPath, copyPath);
                    }
                    if (result)
                    {
                        var msg = new VideoMessage { MessageId = notSendMessageId, MediaUrl = copyPath, MessageType = (int)MessageType.Video, SendUserId = Settings.Current.ChatService.GetUserId() };
                        return await Task.FromResult<IEnumerable<MessageBase>>(new List<MessageBase> { msg });
                    }
                    return await Task.FromResult<IEnumerable<MessageBase>>(null);
                }
            }
            return await Task.FromResult<IEnumerable<MessageBase>>(null);
        }
    }
}
