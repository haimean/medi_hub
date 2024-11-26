namespace MediHub.Web.Aws.Dtos
{
   public class GetS3SignedUrlRequest
   {
      public string Key { get; set; }
      public int Hours { get; set; }
      public string Bucket { get; set; }
   }
}
