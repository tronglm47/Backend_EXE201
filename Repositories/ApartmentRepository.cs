using Repositories.Basic;
using Repositories.Data;
using Repositories.Models;

namespace Repositories
{
    public interface IApartmentRepository : IGenericRepository<Apartment>
    {
    }
    public class ApartmentRepository : GenericRepository<Apartment>, IApartmentRepository
    {
        public ApartmentRepository() : base() { }
        public ApartmentRepository(VLivingDbContext context) : base(context) { }
        
    }
}
