namespace DashboardApi.Application.Email.Request
{
    public class SendEmailRequest
    {
        public List<string> To { get; set; }
        public List<string>? CC { get; set; }
        public string? Subject { get; set; }
        public string? EmailContent { get; set; }
        public List<AttachmentEmail>? Attachments { get; set; } = new List<AttachmentEmail>();
    }

    public class SendMailCommandRequest
    {
        public List<string> to { set; get; } = new List<string>();
        public List<string> cc { set; get; } = new List<string>();
        public string subject { set; get; }
        public string emailContent { set; get; }
        public List<AttachmentEmail> attachments { set; get; } = new List<AttachmentEmail>();
    }

    public class AttachmentEmail
    {
        public string filename { set; get; }
        public string path { set; get; }
    }
}
