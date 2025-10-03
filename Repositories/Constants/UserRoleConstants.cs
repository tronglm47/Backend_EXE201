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

        // Role combinations for authorization
        public const string AdminOrManager = AdminRole + "," + ManagerRole;
        
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
    }
}
