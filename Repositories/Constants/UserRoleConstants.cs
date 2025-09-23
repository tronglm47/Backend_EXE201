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
        
        // Helper method to get all roles
        public static string[] GetAllRoles()
        {
            return new string[]
            {
                AdminRole,
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
    }
}
