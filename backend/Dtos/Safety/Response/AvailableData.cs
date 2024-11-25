using System.ComponentModel.DataAnnotations.Schema;

namespace DashboardApi.Dtos.Safety.Response
{
    public class AvailableData
    {
        public DateTime DateCreated { get; set; }
        public double NationalAverage { get; set; }
        public bool IsDeleted { get; set; }
    }
}
