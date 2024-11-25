using System.ComponentModel.DataAnnotations.Schema;

namespace DashboardApi.Models
{

    public class KPRReason
    {
        public string KprReasonType { get; set; }

        public string KprReason { get; set;}

        public string ProjectId { get; set; }

        public DateTime? TermDate { get; set; }  

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string UpdatedBy { get; set; }

    }

}
