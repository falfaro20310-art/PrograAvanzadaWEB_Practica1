namespace VitalApp_API.Models
{
    public class MessageResponseModel
    {
        public int MessageId { get; set; }
        public int ConsultationId { get; set; }
        public int SenderUserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string SenderName { get; set; } = string.Empty;
    }
}
