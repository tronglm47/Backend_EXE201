using Microsoft.AspNetCore.Authorization;
using Repositories.Constants;

namespace VLivingAPI.Authorization
{
    /// <summary>
    /// Custom authorization attribute for business-specific role combinations
    /// </summary>
    public class BusinessAuthorizeAttribute : AuthorizeAttribute
    {
        public BusinessAuthorizeAttribute(BusinessRole businessRole)
        {
            switch (businessRole)
            {
                case BusinessRole.MasterDataManagement:
                    Roles = UserRoleConstants.BusinessRoles.MasterDataManagement;
                    break;
                case BusinessRole.PostManagement:
                    Roles = UserRoleConstants.BusinessRoles.PostManagement;
                    break;
                case BusinessRole.ReadOnlyAccess:
                    Roles = UserRoleConstants.BusinessRoles.ReadOnlyAccess;
                    break;
                case BusinessRole.FullSystemAccess:
                    Roles = UserRoleConstants.BusinessRoles.FullSystemAccess;
                    break;
                case BusinessRole.ContentModeration:
                    Roles = UserRoleConstants.BusinessRoles.ContentModeration;
                    break;
                default:
                    Roles = UserRoleConstants.UserRole;
                    break;
            }
        }
    }

    public enum BusinessRole
    {
        MasterDataManagement,    // Admin, Manager only
        PostManagement,          // Users who can create/edit posts
        ReadOnlyAccess,          // Users who can only read
        FullSystemAccess,        // Admin only
        ContentModeration        // Admin, Manager, Moderator
    }
}