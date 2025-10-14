using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.Data;
using Repositories.Models;

namespace Repositories
{
    public class ChatUsageRepository : GenericRepository<ChatUsage>, IChatUsageRepository
    {
        public ChatUsageRepository(VLivingDbContext context) : base(context)
        {
        }

        public async Task<int> GetUsageCountByIpAsync(string ipAddress, DateTime from, DateTime to)
        {
            return await _context.Set<ChatUsage>()
                .CountAsync(u => u.IpAddress == ipAddress && 
                               u.RequestTime >= from && 
                               u.RequestTime <= to);
        }

        public async Task<int> GetUsageCountByUserAsync(int userId, DateTime from, DateTime to)
        {
            return await _context.Set<ChatUsage>()
                .CountAsync(u => u.UserId == userId && 
                               u.RequestTime >= from && 
                               u.RequestTime <= to);
        }

        public async Task LogChatUsageAsync(ChatUsage usage)
        {
            await _context.Set<ChatUsage>().AddAsync(usage);
            await _context.SaveChangesAsync();
        }
    }
}