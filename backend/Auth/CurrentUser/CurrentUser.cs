namespace MediHub.Web.Auth.CurrentUser
{
    public interface ICurrentUser
    {
        string GetEmail();
        Guid GetId();
        string GetToken();
    }

    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContext)
        {

            _httpContextAccessor = httpContext;
        }


        public string GetEmail()
        {
            try
            {
                return _httpContextAccessor?.HttpContext?.Items["User"]?.ToString();
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public string GetToken()
        {
            try
            {
                return _httpContextAccessor?.HttpContext?.Request.Headers["Authorization"];
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public Guid GetId()
        {
            var idString = _httpContextAccessor?.HttpContext?.Items["UserId"]?.ToString().ToUpper();

            Guid.TryParse(idString, out var userId);
            return userId;
        }
    }
}
