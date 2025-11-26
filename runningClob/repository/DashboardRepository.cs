using Microsoft.EntityFrameworkCore;
using runningClob.Data;
using runningClob.interfaces;
using runningClob.Models;
using System.Security.Claims;

namespace runningClob.repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            // Fixed: Removed the duplicate assignment
        }
        public async Task<AppUser> GetUserById(string id) 
        { 
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }


        public async Task<List<Club>> GetAllUserClubs()
        {
            var currentUserId = GetCurrentUserId();

            // Since AppUserId is string, we can compare directly
            var userClubs = await _context.Clubs
                .Where(c => c.AppUserId == currentUserId) // Direct string comparison
                .Include(c => c.Address) // Include related data if needed
                .Include(c => c.AppUser) // Include user data if needed
                .ToListAsync();

            return userClubs;
        }

        public async Task<List<Race>> GetAllUserRaces()
        {
            var currentUserId = GetCurrentUserId();

            var userRaces = await _context.Races
                .Where(r => r.AppUserId == currentUserId) // Direct string comparison
                .Include(r => r.Address) // Include related data if needed
                .Include(r => r.AppUser) // Include user data if needed
                .ToListAsync();

            return userRaces;
        }

        private string GetCurrentUserId()
        {
            // This returns the string user ID from ASP.NET Identity
            return _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // Additional helpful methods
        public async Task<int> GetUserClubCount()
        {
            var currentUserId = GetCurrentUserId();
            return await _context.Clubs
                .CountAsync(c => c.AppUserId == currentUserId);
        }

        public async Task<int> GetUserRaceCount()
        {
            var currentUserId = GetCurrentUserId();
            return await _context.Races
                .CountAsync(r => r.AppUserId == currentUserId);
        }
        public async Task<AppUser> GetByIdNoTracking(string id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        public bool UpdateUser(AppUser user)
        {
            _context.Users.Update(user);
            return Save();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}