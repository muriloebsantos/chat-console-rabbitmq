using System;

namespace ChatRoom
{

    public class Message
    {
        public Guid UserId { get; private set; }
        public string Content { get; private set; }
        public string UserName { get; private set; }

        public Message(Guid userId, string userName, string content)
        {
            UserId = userId;
            UserName = userName;
            Content = content;
        }
    }
}