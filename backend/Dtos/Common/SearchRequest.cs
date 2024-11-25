using System.Collections.Generic;

namespace QAQCApi.Dtos.Common
{
    public class SearchRequest
    {
        public string SortBy { get; set; }

        public string FilterBy { get; set; }
        public List<string> Emails { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
