namespace DashboardApi.Auth.PermisionChecker.Dtos
{
    public class CheckPermissionResponse
    {
        public string IsSuccessful { get; set; }
        public bool Data { get; set; }
        public string Message { get; set; }
    }
}
