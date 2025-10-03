using VLivingAPI.Repositories.Data;

namespace Repositories.Basic
{
    public interface IUnitOfWork : IDisposable
    {
        
        Task<int> SaveChangesAsync();
        int SaveChanges();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly VLivingDbContext _context;
       

        public UnitOfWork(VLivingDbContext context)
        {
            _context = context;
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