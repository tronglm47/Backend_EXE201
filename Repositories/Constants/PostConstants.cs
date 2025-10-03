namespace Repositories.Constants
{
    public class PostTypeConstants
    {
        // Post types
        public const string FindRoom = "FindRoom";      // User looking for room
        public const string ForRent = "ForRent";        // Landlord offering room for rent
        public const string ForSale = "ForSale";        // Future: Property for sale
        
        // Helper method to get all post types
        public static string[] GetAllPostTypes()
        {
            return new string[]
            {
                FindRoom,
                ForRent,
                ForSale
            };
        }
        
        // Helper method to check if post type is valid
        public static bool IsValidPostType(string postType)
        {
            return GetAllPostTypes().Contains(postType, StringComparer.OrdinalIgnoreCase);
        }
    }

    public class PostStatusConstants
    {
        // Post statuses
        public const string Active = "Active";          // Post is active and visible
        public const string Inactive = "Inactive";      // Post is hidden
        public const string Pending = "Pending";        // Waiting for approval
        public const string Approved = "Approved";      // Approved by admin
        public const string Rejected = "Rejected";      // Rejected by admin
        public const string Closed = "Closed";          // Post is closed (rented/sold)
        public const string Deleted = "Deleted";        // Soft deleted
        
        // Helper method to get all statuses
        public static string[] GetAllStatuses()
        {
            return new string[]
            {
                Active,
                Inactive,
                Pending,
                Approved,
                Rejected,
                Closed,
                Deleted
            };
        }
        
        // Helper method to check if status is valid
        public static bool IsValidStatus(string status)
        {
            return GetAllStatuses().Contains(status, StringComparer.OrdinalIgnoreCase);
        }
    }
}
