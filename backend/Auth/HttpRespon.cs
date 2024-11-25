namespace DashboardApi.Auth
{
    public class HttpRespon<T>
    {
        public bool IsSuccessful { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }
}
