using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VLivingAPI.Repositories.Data.Models;
using EVCS.Repositories.HuyCG.Interfaces;
using EVCS.Repositories.HuyCG.Basic;

namespace EVCS.Repositories.HuyCG.Basic
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly VLivingDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly Dictionary<Type, object> _repositories;

        // Specific repository properties
        private IGenericRepository<Property>? _properties;
        private IGenericRepository<Post>? _posts;
        private IGenericRepository<Activity>? _activities;
        private IGenericRepository<Ad>? _ads;
        private IGenericRepository<AdRequest>? _adRequests;
        private IGenericRepository<Booking>? _bookings;
        private IGenericRepository<Location>? _locations;
        private IGenericRepository<Message>? _messages;
        private IGenericRepository<Notification>? _notifications;
        private IGenericRepository<Payment>? _payments;
        private IGenericRepository<RoommateMatch>? _roommateMatches;
        private IGenericRepository<RoommatePreference>? _roommatePreferences;
        private IGenericRepository<SubscriptionPlan>? _subscriptionPlans;
        private IGenericRepository<UserSubscription>? _userSubscriptions;

        public UnitOfWork(VLivingDbContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        // Generic repository method
        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
            {
                return (IGenericRepository<T>)_repositories[typeof(T)];
            }

            var repository = new GenericRepository<T>(_context);
            _repositories[typeof(T)] = repository;
            return repository;
        }

        // Specific repository properties
        public IGenericRepository<Property> Properties =>
            _properties ??= new GenericRepository<Property>(_context);

        public IGenericRepository<Post> Posts =>
            _posts ??= new GenericRepository<Post>(_context);

        public IGenericRepository<Activity> Activities =>
            _activities ??= new GenericRepository<Activity>(_context);

        public IGenericRepository<Ad> Ads =>
            _ads ??= new GenericRepository<Ad>(_context);

        public IGenericRepository<AdRequest> AdRequests =>
            _adRequests ??= new GenericRepository<AdRequest>(_context);

        public IGenericRepository<Booking> Bookings =>
            _bookings ??= new GenericRepository<Booking>(_context);

        public IGenericRepository<Location> Locations =>
            _locations ??= new GenericRepository<Location>(_context);

        public IGenericRepository<Message> Messages =>
            _messages ??= new GenericRepository<Message>(_context);

        public IGenericRepository<Notification> Notifications =>
            _notifications ??= new GenericRepository<Notification>(_context);

        public IGenericRepository<Payment> Payments =>
            _payments ??= new GenericRepository<Payment>(_context);

        public IGenericRepository<RoommateMatch> RoommateMatches =>
            _roommateMatches ??= new GenericRepository<RoommateMatch>(_context);

        public IGenericRepository<RoommatePreference> RoommatePreferences =>
            _roommatePreferences ??= new GenericRepository<RoommatePreference>(_context);

        public IGenericRepository<SubscriptionPlan> SubscriptionPlans =>
            _subscriptionPlans ??= new GenericRepository<SubscriptionPlan>(_context);

        public IGenericRepository<UserSubscription> UserSubscriptions =>
            _userSubscriptions ??= new GenericRepository<UserSubscription>(_context);

        // Save changes methods
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        // Transaction management - Async
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        // Transaction management - Sync
        public void BeginTransaction()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }
            _transaction = _context.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                _context.SaveChanges();
                _transaction.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void RollbackTransaction()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                _transaction.Rollback();
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        // Dispose
        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}