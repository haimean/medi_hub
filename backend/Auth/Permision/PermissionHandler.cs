using MediHub.Web.Auth.PermisionChecker;
using Microsoft.AspNetCore.Authorization;

namespace MediHub.Web.Auth.Permision
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionChecker _permissionChecker;

        public PermissionHandler(IPermissionChecker permissionChecker)
        {
            _permissionChecker = permissionChecker;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                context.Fail();
                return;
            }

            context.Succeed(requirement);

            //var userId = new Guid(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            //   var ip = context.User.FindFirst(ClaimTypes.Sid)?.Value;

            //   var isGranted = await _permissionChecker.IsGrantedAsync(userId, requirement.PermissionEntity, ip);
            //   if (isGranted)
            //   {
            //       context.Succeed(requirement);
            //   }
        }
    }
}
