namespace Repositories.Constants
{
    public class UserRoleConstants
    {
        // Core system roles
        public const string AdminRole = "admin";
        public const string ManagerRole = "manager";
        public const string UserRole = "user";
        public const string LandLordRole = "lander";
        public const string AgentRole = "agent";

        // Business role combinations
        public const string AdminOrManager = AdminRole + "," + ManagerRole;
        public const string LandLordOrAgent = LandLordRole + "," + AgentRole;
        public const string AllAuthenticatedUsers = AdminRole + "," + ManagerRole + "," + UserRole + "," + LandLordRole + "," + AgentRole;
        public const string PostCreators = UserRole + "," + LandLordRole + "," + AgentRole;
        
        // Helper method to get all roles
        public static string[] GetAllRoles()
        {
            return new string[]
            {
                AdminRole,
                ManagerRole,
                UserRole,
                LandLordRole,
                AgentRole
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

        /// <summary>
        /// Normalize and validate role input. Returns the proper role or default to "user"
        /// </summary>
        /// <param name="inputRole">Role input from user (can be "landlord", "lander", "agent", etc.)</param>
        /// <returns>Normalized role constant (lander, agent, or user as default)</returns>
        public static string NormalizeRole(string? inputRole)
        {
            if (string.IsNullOrWhiteSpace(inputRole))
            {
                return UserRole; // Default to "user"
            }

            var normalizedInput = inputRole.Trim().ToLowerInvariant();

            // Check for landlord variants
            if (normalizedInput == "landlord" || normalizedInput == "lander")
            {
                return LandLordRole; // Return "lander"
            }

            // Check for agent
            if (normalizedInput == "agent")
            {
                return AgentRole; // Return "agent"
            }

            // If not landlord or agent, default to user
            return UserRole;
        }
    }
}
