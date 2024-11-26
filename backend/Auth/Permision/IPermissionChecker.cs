namespace MediHub.Web.Auth.PermisionChecker
{
   public interface IPermissionChecker
   {
      Task<bool> HasPermission(string permission);
      Task<List<string>> GetPermissions();
      Task<bool> IsGrantedAsync(string email, string permission);
      List<string> Permissions { get; set; }
   }
}
