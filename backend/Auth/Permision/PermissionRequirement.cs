using Microsoft.AspNetCore.Authorization;

namespace MediHub.Web.Auth.Permision
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permission) => Permission = permission;
        public string Permission { get; }
    }
}
