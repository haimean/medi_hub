using static DashboardApi.Auth.PermissionConstants;
namespace DashboardApi.Auth
{
   public class PermissionProvider : IPermissionProvider
   {
      public IReadOnlyList<string> GetAll()
      {
         return new List<string>
          {
              DASHBOARD_SAFETY_VIEW_REPORT
          };
      }
   }

   public interface IPermissionProvider
   {
      IReadOnlyList<string> GetAll();
   }
}
