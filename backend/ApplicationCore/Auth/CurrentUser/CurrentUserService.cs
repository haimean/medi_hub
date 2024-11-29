namespace MediHub.Web.ApplicationCore.Auth.CurrentUser
{
    public interface ICurrentUser
    {
        string GetEmail();
        Guid GetId();
        string GetToken();
    }

    public class CurrentUserService : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContext)
        {

            _httpContextAccessor = httpContext;
        }


        public string GetEmail()
        {
            try
            {
                return _httpContextAccessor?.HttpContext?.Items["User"]?.ToString();
            }
            catch (Exception)
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
            catch (Exception)
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
