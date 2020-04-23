﻿using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace Entap.Chat
{
    [Preserve(AllMembers = true)]
    /// <summary>
    /// メッセージ
    /// </summary>
    public class MessageBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public MessageBase()
        {
        }

        public MessageBase(MessageBase notSendMessage)
        {
            MessageId = notSendMessage.MessageId;
            SendDateTime = notSendMessage.SendDateTime;
            Text = notSendMessage.Text;
            MediaUrl = notSendMessage.MediaUrl;
            UserIcon = notSendMessage.UserIcon;
            MessageType = notSendMessage.MessageType;
            SendUserId = notSendMessage.SendUserId;
            AlreadyReadCount = 0;
            ResendVisible = true;
            //NotSendId = notSendMessage.Id;
        }

        public int MessageId { get; set; }
        
        private DateTime sendDateTime;
        public DateTime SendDateTime
        {
            get
            {
                return sendDateTime;
            }
            set
            {
                if (sendDateTime != value)
                {
                    sendDateTime = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("SendDateTime"));
                }
            }
        }

        public string Text { get; set; }
        public string MediaUrl { get; set; }
        public string UserIcon { get; set; }
        /// <summary>
        /// 1:テキスト, 2:画像, 3:動画
        /// </summary>
        public int MessageType { get; set; }
        public string SendUserId { get; set; }

        private int alreadyReadCount;
        public int AlreadyReadCount
        {
            get
            {
                return alreadyReadCount;
            }
            set
            {
                if (alreadyReadCount != value)
                {
                    alreadyReadCount = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("AlreadyReadCount"));
                }
            }
        }

        private bool resendVisible;
        public bool ResendVisible
        {
            get
            {
                return resendVisible;
            }
            set
            {
                if (resendVisible != value)
                {
                    resendVisible = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("ResendVisible"));
                }
            }
        }

        public int NotSendId { get; set; }

        private bool dateVisible;
        public bool DateVisible
        {
            get
            {
                return dateVisible;
            }
            set
            {
                if (dateVisible != value)
                {
                    dateVisible = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("DateVisible"));
                }
            }
        }
    }
}
