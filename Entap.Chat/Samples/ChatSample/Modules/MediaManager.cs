﻿using System;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ChatSample
{
    public class MediaManager
    {
        public MediaManager()
        {
        }

        public static async Task VideoShare(string videoPath)
        {
            var mediaFolderPath = DependencyService.Get<IFileService>().GetMediaFolderPath();
            string filePath = mediaFolderPath + "/videofile.mp4";

            await OpenShareMenu(videoPath, filePath);
        }

        public static async Task ImageShare(string imagePath)
        {
            var mediaFolderPath = DependencyService.Get<IFileService>().GetMediaFolderPath();
            var extension = System.IO.Path.GetExtension(imagePath);
            string filePath = mediaFolderPath;
            if (extension.ToLower() == ".jpeg" || extension.ToLower() == ".jpg")
            {
                filePath += "/imgfile.jpeg";
            }
            else if (extension.ToLower() == ".png")
            {
                filePath += "/imgfile.png";
            }
            else if (extension.ToLower() == ".pdf")
            {
                filePath += "/imgfile.pdf";
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("エラー", "こちらのファイルは表示できません", "閉じる");
                return;
            }
            await OpenShareMenu(imagePath, filePath);
        }

        static async Task OpenShareMenu(string source, string copyPath)
        {
            bool result;
            if (source.Contains("http://") || source.Contains("https://"))
            {
                result = await DownloadWebFile(source, copyPath);
            }
            else
            {
                result = FileManager.FileCopy(source, copyPath);
            }

            if (!result)
            {
                await Application.Current.MainPage.DisplayAlert("エラー", "ファイルが取得できませんでした", "閉じる");
                return;
            }

            string str = "error";
            DependencyService.Get<IFileService>().OpenShareMenu(copyPath, ref str);
        }

        public static Task<bool> DownloadWebFile(string dlPath, string savePath)
        {
            var cmp = new TaskCompletionSource<bool>();
            bool downloadWebImageFileCmp = false;
            try
            {
                WebClient client = new WebClient();
                client.DownloadFileAsync(new Uri(dlPath), savePath);
                client.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
                {
                    try
                    {
                        cmp.SetResult(true);
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("DownloadFileCompleted Error: " + ex);
                    }
                    downloadWebImageFileCmp = true;
                };

                Task.Run(async () =>
                {
                    await Task.Delay(30000);
                    if (downloadWebImageFileCmp)
                        return;
                    client.CancelAsync();
                    cmp.SetResult(false);
                });
            }
            catch (Exception ex)
            {
                cmp.SetResult(false);
                System.Diagnostics.Debug.WriteLine("FileDownLoadError : " + ex);
            }
            return cmp.Task;
        }

        public static async Task ImageDownload(string imageUrl)
        {
            var dlFolderPath = DependencyService.Get<IFileService>().GetDownloadFolderPath();
            var extension = System.IO.Path.GetExtension(imageUrl);
            string filePath = dlFolderPath;
            if (extension.ToLower() == ".jpeg" || extension.ToLower() == ".jpg")
            {
                filePath += "/" + Guid.NewGuid() + ".jpeg";
            }
            else if (extension.ToLower() == ".pdf")
            {
                filePath += "/" + Guid.NewGuid() + ".pdf";
            }
            else
            {
                filePath += "/" + Guid.NewGuid() + ".png";
            }
            bool? dlResult;
            if (Device.RuntimePlatform == Device.Android)
            {
                dlResult = await DownloadWebFile(imageUrl, filePath);
            }
            else
            {
                if (imageUrl.Contains("http://") || imageUrl.Contains("https://"))
                {
                    var mediaFolderPath = DependencyService.Get<IFileService>().GetMediaFolderPath();
                    if (extension.ToLower() == ".jpeg" || extension.ToLower() == ".jpg")
                    {
                        mediaFolderPath += "/" + Guid.NewGuid() + ".jpeg";
                    }
                    else if (extension.ToLower() == ".pdf")
                    {
                        mediaFolderPath += "/" + Guid.NewGuid() + ".pdf";
                    }
                    else
                    {
                        mediaFolderPath += "/" + Guid.NewGuid() + ".png";
                    }
                    var result = await DownloadWebFile(imageUrl, mediaFolderPath);
                    if (!result)
                    {
                        await Application.Current.MainPage.DisplayAlert("エラー", "ファイルが取得できませんでした", "閉じる");
                        return;
                    }
                    dlResult = DependencyService.Get<IFileService>().SaveImageiOSLibrary(mediaFolderPath);
                }
                else
                {
                    dlResult = DependencyService.Get<IFileService>().SaveImageiOSLibrary(imageUrl);
                }
            }

            if (dlResult == true)
                await Application.Current.MainPage.DisplayAlert("", "保存しました", "閉じる");
            else if (dlResult == false)
                await Application.Current.MainPage.DisplayAlert("", "保存できませんでした", "閉じる");
        }

        public static async Task VideoDownload(string videoUrl)
        {
            var dlFolderPath = DependencyService.Get<IFileService>().GetDownloadFolderPath();
            string filePath = dlFolderPath + "/" + Guid.NewGuid() + ".mp4";
            
            bool? dlResult;
            if (Device.RuntimePlatform == Device.Android)
            {
                dlResult = await DownloadWebFile(videoUrl, filePath);
            }
            else
            {
                if (videoUrl.Contains("http://") || videoUrl.Contains("https://"))
                {
                    var mediaFolderPath = DependencyService.Get<IFileService>().GetMediaFolderPath();
                    mediaFolderPath += "/" + Guid.NewGuid() + ".mp4";
                    var result = await DownloadWebFile(videoUrl, mediaFolderPath);
                    if (!result)
                    {
                        await Application.Current.MainPage.DisplayAlert("エラー", "ファイルが取得できませんでした", "閉じる");
                        return;
                    }
                    dlResult = DependencyService.Get<IFileService>().SaveVideoiOSLibrary(mediaFolderPath);
                }
                else
                {
                    dlResult = DependencyService.Get<IFileService>().SaveVideoiOSLibrary(videoUrl);
                }
            }

            if (dlResult == true)
                await Application.Current.MainPage.DisplayAlert("", "保存しました", "閉じる");
            else if (dlResult == false)
                await Application.Current.MainPage.DisplayAlert("", "保存できませんでした", "閉じる");
        }
    }
}
