namespace DashboardApi.Dtos.DashboardWorker.Response
{
   public class ProductivityRane
   {
      public ProductivityRane(string name, int count)
      {
         this.name = name;
         this.count = count;
      }

      public string name { get; set; }
      public int count { get; set; }
   }
}
