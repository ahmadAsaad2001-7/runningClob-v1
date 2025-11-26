using Microsoft.EntityFrameworkCore;
using runningClob.Data;
using runningClob.Data.Enum;
using runningClob.interfaces;
using runningClob.Models;

namespace runningClob.repository.RaceRepositroy
{
    public class RaceRepository : IRaceRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RaceRepository> _logger;

        public RaceRepository(AppDbContext context, ILogger<RaceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool Add(Race race)
        {
            _context.Add(race);
            return Save();
        }


        public async Task<bool> DeleteAsync(Race race)
        {
            try
            {
                _context.Remove(race);
                return await SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting club");
                return false;
            }
        }

        public async Task<IEnumerable<Race>> GetAll()
        {
            return await _context.Races.ToListAsync();
        }

        public async Task<IEnumerable<Race>> GetAllRacesByCity(string city)
        {



            return await _context.Races.Where(c => c.Address.City.Contains(city)).ToListAsync();
        }

        public async Task<Race?> GetByIdAsync(int id, bool trackEntity = true)
        {
            try
            {
                var query = _context.Races
                    .Include(r => r.Address)
                    .Include(r => r.AppUser);

                var race = trackEntity
                    ? await query.FirstOrDefaultAsync(c => c.Id == id)
                    : await query.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

                return race;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting club by id: {Id}", id);
                return null;
            }
        }

        public async Task<Race?> GetByIdAsyncNoTracking(int id)
        {
            return await _context.Races.Include(i => i.Address).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Races.CountAsync();
        }

        public async Task<int> GetCountByCategoryAsync(RaceCategory category)
        {
            return await _context.Races.CountAsync(r => r.RaceCategory == category);
        }

        public async Task<IEnumerable<Race>> GetSliceAsync(int offset, int size)
        {
            return await _context.Races.Include(a => a.Address).Skip(offset).Take(size).ToListAsync();
        }

        public async Task<IEnumerable<Race>> GetRacesByCategoryAndSliceAsync(RaceCategory category, int offset, int size)
        {
            return await _context.Races
                .Where(r => r.RaceCategory == category)
                .Include(a => a.Address)
                .Skip(offset)
                .Take(size)
                .ToListAsync();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0;
        }
        public async Task<bool> SaveAsync()
        {
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database");
                return false;
            }
        }
        public async Task<bool> UpdateAsync(Race race)
        {
            try
            {
                _context.Update(race);
                return await SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating race");
                return false;
            }
        }
    }
}