using runningClob.Data.Enum;
using runningClob.Models;

namespace runningClob.interfaces
{
    public interface IRaceRepository
    {
        Task<int> GetCountAsync();

        Task<int> GetCountByCategoryAsync(RaceCategory category);

        Task<Race?> GetByIdAsync(int id, bool trackEntity = true);

        Task<Race?> GetByIdAsyncNoTracking(int id);

        Task<IEnumerable<Race>> GetAll();

        Task<IEnumerable<Race>> GetAllRacesByCity(string city);

        Task<IEnumerable<Race>> GetSliceAsync(int offset, int size);

        Task<IEnumerable<Race>> GetRacesByCategoryAndSliceAsync(RaceCategory category, int offset, int size);

        bool Add(Race race);

        Task<bool> UpdateAsync(Race race);

         Task<bool> DeleteAsync(Race race);

        bool Save();
        Task<bool> SaveAsync();
    }
}