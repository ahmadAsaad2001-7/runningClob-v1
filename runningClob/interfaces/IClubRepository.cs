using runningClob.Data.Enum;
using runningClob.Models;

namespace runningClob.interfaces
{
    public interface IClubRepository
    {
        // ====== Basic CRUD Operations ======
        public Task<bool> AddAsync(Club club);
        public Task<bool> UpdateAsync(Club club);
        public Task<bool> DeleteAsync(Club club);
        public Task<bool> SaveAsync();

        // ====== Get Operations ======
        public Task<IEnumerable<Club>> GetAllAsync();
        public Task<Club?> GetByIdAsync(int id, bool trackEntity = true);
        public Task<IEnumerable<Club>> GetClubByCityAsync(string city);
        public Task<IEnumerable<Club>> GetClubsByStateAsync(string state);
        public Task<IEnumerable<Club>> GetClubsByLocationAsync(string country, string state, string city);
        public Task<IEnumerable<Club>> GetClubsByCountryAsync(string country); // Added Async
        public Task<IEnumerable<Club>> GetClubsByProgressiveLocationAsync(string country, string state = null, string city = null);

        // ====== Pagination ======
        public Task<IEnumerable<Club>> GetSliceAsync(int offset, int size);
        public Task<IEnumerable<Club>> GetClubsByCategoryAndSliceAsync(ClubCategory category, int offset, int size);

        // ====== Count Operations ======
        public Task<int> GetCountAsync();
        public Task<int> GetCountByCategoryAsync(ClubCategory category);

        // ====== Location Data ======
        //    public Task<IEnumerable<State>> GetAllStatesAsync();
        //    public Task<IEnumerable<City>> GetAllCitiesByStateAsync(string state);
        //}
    }
}