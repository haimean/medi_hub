namespace DashboardApi.Dtos.DashboardWorker.Response
{
   public class EntStatic
   {

      public EntStatic(string key, string value)
      {
         this.key = key;
         this.value = value;
      }
      public string key { get; set; }
      public string value { get; set; }
   }
}
