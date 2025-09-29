using Repositories.Basic;

namespace Services
{
    public interface IPostService
    {
        // Define methods for post-related operations here
    }
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PostService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}