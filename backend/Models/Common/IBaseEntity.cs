using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediHub.Web.Models.Common
{
    public abstract class IBaseEntity : IBaseEntity<Guid>
    {

    }

    public abstract class IBaseEntity<TKey> : BaseEntity where TKey : IEquatable<TKey>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("name")]
        public string? Name { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;


        [Column("updated_by")]
        public string? UpdatedBy { get; set; }


        private DateTime _UpdatedDate = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedDate
        {
            get
            {
                return _UpdatedDate;
            }
            set
            {
                _UpdatedDate = (value != null ? (DateTime)value : DateTime.UtcNow);
            }
        }
    }
}
