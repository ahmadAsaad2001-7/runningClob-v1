using Microsoft.EntityFrameworkCore;
using runningClob.Data;
using runningClob.Data.Enum;
using runningClob.interfaces;
using runningClob.Models;
using runningClob.Services;

namespace runningClob.repository.RaceRepositroy
{
    public class RaceRepository : IRaceRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RaceRepository> _logger;
        private readonly ICountryAliasService _countryAliasService;

        public RaceRepository(AppDbContext context, ILogger<RaceRepository> logger,ICountryAliasService countryAliasService)
        {
            _context = context;
            _logger = logger;
            _countryAliasService = countryAliasService;
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
        public async Task<IEnumerable<Race>> GetRacesByCountryAsync(string country)
        {
            try
            {
                _logger.LogInformation("🔍 RACE REPOSITORY: Starting search for: '{Country}'", country);

                if (string.IsNullOrWhiteSpace(country))
                {
                    _logger.LogWarning("🔍 RACE REPOSITORY: Country parameter is empty");
                    return new List<Race>();
                }

                // STEP 1: Normalize the search country
                var searchCountryCode = _countryAliasService.NormalizeCountry(country);
                _logger.LogInformation("🔍 RACE REPOSITORY: Normalized search country: '{SearchCountryCode}'", searchCountryCode);

                // STEP 2: Get ALL races with addresses (no filtering yet)
                var allRaces = await _context.Races
                    .Include(r => r.Address)
                    .Where(r => r.Address != null) // Only races with addresses
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("🔍 RACE REPOSITORY: Retrieved {Count} races with addresses", allRaces.Count);

                // STEP 3: Filter in memory (more reliable than complex LINQ)
                var filteredRaces = allRaces
                    .Where(r =>
                    {
                        if (string.IsNullOrEmpty(r.Address.Country))
                            return false;

                        // Normalize each race's country and compare
                        var raceCountryCode = _countryAliasService.NormalizeCountry(r.Address.Country);
                        return raceCountryCode == searchCountryCode;
                    })
                    .ToList();

                _logger.LogInformation("🔍 RACE REPOSITORY: Found {FilteredCount} races matching '{SearchCountryCode}'",
                    filteredRaces.Count, searchCountryCode);

                // STEP 4: Debug logging - show what we found
                if (filteredRaces.Any())
                {
                    _logger.LogInformation("🔍 RACE REPOSITORY: Matching races:");
                    foreach (var race in filteredRaces)
                    {
                        var raceCountryCode = _countryAliasService.NormalizeCountry(race.Address.Country);
                        _logger.LogInformation("   ✅ ID: {Id}, Title: {Title}, Country: '{Country}' -> '{Code}'",
                            race.Id, race.Title, race.Address.Country, raceCountryCode);
                    }
                }
                else
                {
                    _logger.LogWarning("🔍 RACE REPOSITORY: NO RACES FOUND for '{SearchCountryCode}'", searchCountryCode);

                    // Log what countries we actually have for debugging
                    var availableCountries = allRaces
                        .Where(r => !string.IsNullOrEmpty(r.Address.Country))
                        .Select(r => new {
                            Original = r.Address.Country,
                            Normalized = _countryAliasService.NormalizeCountry(r.Address.Country)
                        })
                        .Distinct()
                        .ToList();

                    _logger.LogInformation("🔍 RACE REPOSITORY: Available countries in database:");
                    foreach (var countryInfo in availableCountries)
                    {
                        _logger.LogInformation("   - '{Original}' -> '{Normalized}'",
                            countryInfo.Original, countryInfo.Normalized);
                    }
                }

                return filteredRaces;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ RACE REPOSITORY: Error in GetRacesByCountryAsync for '{Country}'", country);
                return new List<Race>();
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