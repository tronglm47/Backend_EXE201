using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Constants
{
    public class UserRoleConstants
    {
        // Core system roles
        public const string AdminRole = "admin";
        public const string ManagerRole = "manager";
        public const string UserRole = "user";
        
        // Property-related roles
        public const string PropertyOwnerRole = "property_owner";
        public const string TenantRole = "tenant";
        public const string AdvertiserRole = "advertiser";
        
        // Content management roles
        public const string ModeratorRole = "moderator";
        
        // Subscription-based roles
        public const string PremiumUserRole = "premium_user";
        public const string BasicUserRole = "basic_user";
        
        // Business roles
        public const string LandlordRole = "landlord";
        public const string RenterRole = "renter";

        // Role combinations for authorization
        public const string AdminOrManager = AdminRole + "," + ManagerRole;
        public const string AdminOrManagerOrModerator = AdminRole + "," + ManagerRole + "," + ModeratorRole;
        public const string AllUsersExceptGuest = UserRole + "," + AdminRole + "," + ManagerRole + "," + ModeratorRole + "," + PremiumUserRole + "," + PropertyOwnerRole + "," + LandlordRole;
        public const string PropertyManagementRoles = PropertyOwnerRole + "," + LandlordRole + "," + AdminRole + "," + ManagerRole;
        public const string ContentCreationRoles = UserRole + "," + PremiumUserRole + "," + PropertyOwnerRole + "," + LandlordRole + "," + AdvertiserRole;
        
        // Helper method to get all roles
        public static string[] GetAllRoles()
        {
            return new string[]
            {
                AdminRole,
                ManagerRole,
                UserRole,
                PropertyOwnerRole,
                TenantRole,
                AdvertiserRole,
                ModeratorRole,
                PremiumUserRole,
                BasicUserRole,
                LandlordRole,
                RenterRole
            };
        }
        
        // Helper method to check if role is valid
        public static bool IsValidRole(string role)
        {
            var allRoles = GetAllRoles();
            return allRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        // Helper method to check if user has any of the required roles
        public static bool HasAnyRole(string[] userRoles, string requiredRoles)
        {
            var required = requiredRoles.Split(',').Select(r => r.Trim()).ToArray();
            return userRoles.Any(userRole => required.Contains(userRole, StringComparer.OrdinalIgnoreCase));
        }

        // Helper method to get roles for specific business scenarios
        public static class BusinessRoles
        {
            public const string MasterDataManagement = AdminRole + "," + ManagerRole;
            public const string PostManagement = UserRole + "," + PremiumUserRole + "," + PropertyOwnerRole + "," + LandlordRole + "," + AdvertiserRole + "," + AdminRole + "," + ManagerRole;
            public const string ReadOnlyAccess = UserRole + "," + TenantRole + "," + RenterRole + "," + BasicUserRole;
            public const string FullSystemAccess = AdminRole;
            public const string ContentModeration = AdminRole + "," + ManagerRole + "," + ModeratorRole;
        }
    }
}
