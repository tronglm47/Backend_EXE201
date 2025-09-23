using VLivingAPI.Repositories.Data.Models;

namespace EVCS.Repositories.HuyCG.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Generic Repository cho tất cả entities
        IGenericRepository<T> Repository<T>() where T : class;
        
        // Specific repositories cho các entities chính (trừ User)
        IGenericRepository<Property> Properties { get; }
        IGenericRepository<Post> Posts { get; }
        IGenericRepository<Activity> Activities { get; }
        IGenericRepository<Ad> Ads { get; }
        IGenericRepository<AdRequest> AdRequests { get; }
        IGenericRepository<Booking> Bookings { get; }
        IGenericRepository<Location> Locations { get; }
        IGenericRepository<Message> Messages { get; }
        IGenericRepository<Notification> Notifications { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<RoommateMatch> RoommateMatches { get; }
        IGenericRepository<RoommatePreference> RoommatePreferences { get; }
        IGenericRepository<SubscriptionPlan> SubscriptionPlans { get; }
        IGenericRepository<UserSubscription> UserSubscriptions { get; }

        // Transaction methods
        Task<int> SaveChangesAsync();
        int SaveChanges();
        
        // Transaction management
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}