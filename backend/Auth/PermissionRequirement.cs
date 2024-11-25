using Microsoft.AspNetCore.Authorization;

namespace DashboardApi.Auth
{
   public class PermissionRequirement : IAuthorizationRequirement
   {
      public PermissionRequirement(string permission) => Permission = permission;
      public string Permission { get; }
   }
}
