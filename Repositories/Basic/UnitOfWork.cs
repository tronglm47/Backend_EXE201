using VLivingAPI.Repositories.Data;

namespace Repositories.Basic
{
    public interface IUnitOfWork : IDisposable
    {
        ISubdivisionRepository Subdivisions { get; }
        Task<int> SaveChangesAsync();
        int SaveChanges();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly VLivingDbContext _context;
        private ISubdivisionRepository? _subdivisionRepository;

        public UnitOfWork(VLivingDbContext context)
        {
            _context = context;
        }

        public ISubdivisionRepository Subdivisions
        {
            get
            {
                _subdivisionRepository ??= new SubdivisionRepository(_context);
                return _subdivisionRepository;
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