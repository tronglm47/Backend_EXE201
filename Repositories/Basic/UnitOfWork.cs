using Microsoft.EntityFrameworkCore.Storage;
using VLivingAPI.Repositories.Data.Models;

namespace Repositories.Basic
{
    public interface IUnitOfWork : IDisposable
    {
        LocationRepository Locations { get; }
        PostAmenityRepository PostAmenity { get; }
        PostRepository Posts { get; }
        PostTypeRepository PostType { get; }
        PropertyTypeRepository PropertyTypes { get; }
        PropertyFormRepository PropertyForms { get; }
        AmenityRepository Amenities { get; }
        
        Task<int> SaveChangesAsync();
        int SaveChanges();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly VLivingDbContext _context;
        
        public LocationRepository Locations { get; private set; }
        public PostAmenityRepository PostAmenity { get; private set; }
        public PostRepository Posts { get; private set; }
        public PostTypeRepository PostType { get; private set; }
        public PropertyTypeRepository PropertyTypes { get; private set; }
        public PropertyFormRepository PropertyForms { get; private set; }
        public AmenityRepository Amenities { get; private set; }

        public UnitOfWork(VLivingDbContext context)
        {
            _context = context;
            Locations = new LocationRepository(context);
            PostAmenity = new PostAmenityRepository(context);
            Posts = new PostRepository(context);
            PostType = new PostTypeRepository(context);
            PropertyTypes = new PropertyTypeRepository(context);
            PropertyForms = new PropertyFormRepository(context);
            Amenities = new AmenityRepository(context);
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