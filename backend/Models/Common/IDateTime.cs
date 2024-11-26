namespace MediHub.Web.Models.Common
{
    public interface IDateTime
    {
        DateTime CreatedAt { get; set; }

        string CreatedBy { get; set; }

        DateTime UpdatedAt { get; set; }

        string UpdatedBy { get; set; }
    }
}
