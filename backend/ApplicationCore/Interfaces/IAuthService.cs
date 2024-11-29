using MediHub.Web.HttpConfig;

namespace MediHub.Web.ApplicationCore.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<ServiceResponse> LoginAsync(string username, string password);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<ServiceResponse> LogoutAsync(string username);
    }
}
