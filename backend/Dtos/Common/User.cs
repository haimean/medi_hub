using System.ComponentModel.DataAnnotations.Schema;

namespace MediHub.Web.Dtos.Common
{
    [NotMapped]
    public class UserPF
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool Confirmed { get; set; }
        public UserPF(string id, string email, bool confirmed)
        {
            Id = id;
            Email = email;
            Confirmed = confirmed;
        }
    }

    [NotMapped]
    public class PublicUserPF
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public PublicUserPF(string email, string name)
        {
            Email = email;
            Name = name;
        }
    }
}

