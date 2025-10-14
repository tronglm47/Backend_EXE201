using Repositories.Models;
using Repositories.Basic;

namespace Repositories
{
    public interface IChatUsageRepository : IGenericRepository<ChatUsage>
    {
        Task<int> GetUsageCountByIpAsync(string ipAddress, DateTime from, DateTime to);
        Task<int> GetUsageCountByUserAsync(int userId, DateTime from, DateTime to);
        Task LogChatUsageAsync(ChatUsage usage);
    }
}