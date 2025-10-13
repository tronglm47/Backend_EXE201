using Repositories.Data;

namespace Repositories.Basic
{
    public interface IUnitOfWork : IDisposable
    {
        IApartmentRepository Apartments { get; }
        IBuildingRepository Buildings { get; }
        IPostUtilityRepository PostUtilities { get; }
        IPostRepository Posts { get; }
        IPostImageRepository PostImages { get; }
        ISubdivisionRepository Subdivisions { get; }
        IUtilityRepository Utilities { get; }
        Task<int> SaveChangesAsync();
        int SaveChanges();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly VLivingDbContext _context;
        private IApartmentRepository? _apartmentRepository;
        private IBuildingRepository? _buildingRepository;
        private IPostRepository? _postRepository;
        private IPostImageRepository? _postImageRepository;
        private IPostUtilityRepository? _postUtilityRepository;
        private ISubdivisionRepository? _subdivisionRepository;
        private IUtilityRepository? _utilityRepository;

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
        public IUtilityRepository Utilities
        {
            get
            {
                _utilityRepository ??= new UtilityRepository(_context);
                return _utilityRepository;
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

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}