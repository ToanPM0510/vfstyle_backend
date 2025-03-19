// DTOs/ChatDTO.cs
using System;
using System.Collections.Generic;

namespace vfstyle_backend.DTOs
{
    public class MessageDTO
    {
        public string Content { get; set; }
        public string Sender { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ConversationDTO
    {
        public int Id { get; set; }
        public List<MessageDTO> Messages { get; set; }
    }

    public class SendMessageDTO
    {
        public int? ConversationId { get; set; }
        public string Content { get; set; }
    }
}