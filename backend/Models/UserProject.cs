using System.ComponentModel.DataAnnotations.Schema;

namespace DashboardApi.Models
{
    public class UserProject
    {
        [Column("id")]
        public Guid ID { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("role_id")]
        public Guid RoleId { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("project_id")]
        public string ProjectId { get; set; }

        [Column("user_created")]
        public string UserCreated { get; set; }

        [Column("is_delete")]
        public bool IsDelete { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("project_name")]
        public string ProjectName { get; set; }

        [Column("project_type")]
        public string ProjectType { get; set; }
    }
}
