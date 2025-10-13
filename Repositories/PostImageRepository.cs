using Repositories.Basic;
using Repositories.Data;
using Repositories.Models;

namespace Repositories
{
    public interface IPostImageRepository : IGenericRepository<PostImage>
    {

    }
    public class PostImageRepository : GenericRepository<PostImage>, IPostImageRepository
    {
        public PostImageRepository() : base() { }
        public PostImageRepository(VLivingDbContext context) : base(context) { }
    }
}
