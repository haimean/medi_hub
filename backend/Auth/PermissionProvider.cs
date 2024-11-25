using static DashboardApi.Auth.PermissionConstants;
namespace DashboardApi.Auth
{
   public class PermissionProvider : IPermissionProvider
   {
      public IReadOnlyList<string> GetAll()
      {
         return new List<string>
          {
             CREATE_PO,
             CREATE_DO,
             VIEW_PO,
             VIEW_DO,
          };
      }
   }

   public interface IPermissionProvider
   {
      IReadOnlyList<string> GetAll();
   }
}
