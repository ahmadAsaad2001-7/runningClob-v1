using runningClob.Models;

namespace runningClob.interfaces
{
    public interface IDashboardRepository
    {
        Task<List<Race>> GetAllUserRaces ();
        Task<List<Club>> GetAllUserClubs ();
        Task<AppUser> GetUserById(string id);
        Task<AppUser> GetByIdNoTracking(string id);
        bool UpdateUser(AppUser user);
        bool Save();
    }
}
