namespace QAQCApi.Dtos.Common
{
    public class SearchDoRequest : SearchRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string nameProject { get; set; }
        public List<string> ProjectIds { get; set; }
        public List<Guid> Ids { get; set; }

    }
}
