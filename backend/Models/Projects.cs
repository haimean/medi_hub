using System.ComponentModel.DataAnnotations.Schema;

namespace DashboardApi.Models
{
    public class Projects
    {
        [Column("id")]
        public string ID { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("image")]
        public string Image { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("trimble_key")]
        public string TrimbleKey { get; set; }

        [Column("issue_key")]
        public string IssueKey { get; set; }

        [Column("use_rfa")]
        public string UseRfa { get; set; }

        [Column("user_created")]
        public string UserCreated { get; set; }

        [Column("is_delete")]
        public bool IsDelete { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("is_all_project")]
        public bool IsAllProject { get; set; }

        [Column("is_default")]
        public bool isDefault { get; set; }

        [Column("status")]
        public string Status { get; set; }
    }
}
