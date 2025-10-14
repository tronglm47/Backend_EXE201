using Repositories.Data;

namespace Repositories.Basic
{
    public interface IUnitOfWork : IDisposable
    {
        IApartmentRepository Apartments { get; }
        IBookingRepository Bookings { get; }
        IBuildingRepository Buildings { get; }
        IPostUtilityRepository PostUtilities { get; }
        IPostRepository Posts { get; }
        IPostImageRepository PostImages { get; }
        IReviewRepository Reviews { get; }
        ISubdivisionRepository Subdivisions { get; }
        IUtilityRepository Utilities { get; }
        IUserRepository Users { get; }
        Task<int> SaveChangesAsync();
        int SaveChanges();
        Task<int> SaveAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly VLivingDbContext _context;
        private IApartmentRepository? _apartmentRepository;
        private IBookingRepository? _bookingRepository;
        private IBuildingRepository? _buildingRepository;
        private IPostRepository? _postRepository;
        private IPostImageRepository? _postImageRepository;
        private IPostUtilityRepository? _postUtilityRepository;
        private IReviewRepository? _reviewRepository;
        private ISubdivisionRepository? _subdivisionRepository;
        private IUtilityRepository? _utilityRepository;
        private IUserRepository? _userRepository;

        public UnitOfWork(VLivingDbContext context)
        {
            _context = context;
        }

        public IApartmentRepository Apartments
        {
            get
            {
                _apartmentRepository ??= new ApartmentRepository(_context);
                return _apartmentRepository;
            }
        }

        public IBookingRepository Bookings
        {
            get
            {
                _bookingRepository ??= new BookingRepository(_context);
                return _bookingRepository;
            }
        }

        public ISubdivisionRepository Subdivisions
        {
            get
            {
                _subdivisionRepository ??= new SubdivisionRepository(_context);
                return _subdivisionRepository;
            }
        }
        public IBuildingRepository Buildings
        {
            get
            {
                _buildingRepository ??= new BuildingRepository(_context);
                return _buildingRepository;
            }
        }
        public IPostRepository Posts
        {
            get
            {
                _postRepository ??= new PostRepository(_context);
                return _postRepository;
            }
        }
        public IPostImageRepository PostImages
        {
            get
            {
                _postImageRepository ??= new PostImageRepository(_context);
                return _postImageRepository;
            }
        }
        public IPostUtilityRepository PostUtilities
        {
            get
            {
                _postUtilityRepository ??= new PostUtilityRepository(_context);
                return _postUtilityRepository;
            }
        }

        public IReviewRepository Reviews
        {
            get
            {
                _reviewRepository ??= new ReviewRepository(_context);
                return _reviewRepository;
            }
        }

        public IUtilityRepository Utilities
        {
            get
            {
                _utilityRepository ??= new UtilityRepository(_context);
                return _utilityRepository;
            }
        }

        public IUserRepository Users
        {
            get
            {
                _userRepository ??= new UserRepository(_context);
                return _userRepository;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}