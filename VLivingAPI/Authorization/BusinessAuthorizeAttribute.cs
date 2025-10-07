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
                    // Admin and Manager can manage master data
                    Roles = UserRoleConstants.AdminOrManager;
                    break;
                case BusinessRole.PostManagement:
                    // Users, Landlords, and Agents can create/edit posts
                    Roles = UserRoleConstants.PostCreators;
                    break;
                case BusinessRole.PropertyManagement:
                    // Landlords and Agents can manage properties (apartments, buildings)
                    Roles = UserRoleConstants.LandLordOrAgent;
                    break;
                case BusinessRole.ReadOnlyAccess:
                    // All authenticated users can read
                    Roles = UserRoleConstants.AllAuthenticatedUsers;
                    break;
                case BusinessRole.FullSystemAccess:
                    // Admin only for critical operations
                    Roles = UserRoleConstants.AdminRole;
                    break;
                case BusinessRole.ContentModeration:
                    // Admin and Manager can moderate content
                    Roles = UserRoleConstants.AdminOrManager;
                    break;
                default:
                    Roles = UserRoleConstants.UserRole;
                    break;
            }
        }
    }

    public enum BusinessRole
    {
        MasterDataManagement,    // Admin, Manager only - for utilities, subdivisions
        PostManagement,          // Users, Landlords, Agents who can create/edit posts
        PropertyManagement,      // Landlords, Agents - for apartments, buildings
        ReadOnlyAccess,          // All authenticated users who can read
        FullSystemAccess,        // Admin only for critical operations
        ContentModeration        // Admin, Manager for content moderation
    }
}