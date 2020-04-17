﻿using System;
using SQLite;

namespace Entap.Chat
{
    /// <summary>
    /// 未送信メッセージ
    /// </summary>
    public class NotSendMessage : MessageBase
    {
        public NotSendMessage()
        {
        }
        public NotSendMessage(int roomId,MessageBase messageBase)
        {
            MessageId = messageBase.MessageId;
            SendDateTime = messageBase.SendDateTime;
            Text = messageBase.Text;
            MediaUrl = messageBase.MediaUrl;
            UserIcon = messageBase.UserIcon;
            MessageType = messageBase.MessageType;
            SendUserId = messageBase.SendUserId;
            AlreadyReadCount = 0;
            ResendVisible = true;
            RoomId = roomId;
        }
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int RoomId { get; set; }
    }
}
