using Microsoft.EntityFrameworkCore;
using runningClob.Data;
using runningClob.Data.Enum;
using runningClob.interfaces;
using runningClob.Models;
using runningClob.Services;

namespace runningClob.repository.ClubRepository
{
    public class ClubRepository : IClubRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClubRepository> _logger;
        private readonly ICountryAliasService _countryAliasService;

        public ClubRepository(AppDbContext context, ILogger<ClubRepository> logger,ICountryAliasService countryAliasService)
        {
            _countryAliasService = countryAliasService;
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddAsync(Club club)
        {
            try
            {
                await _context.AddAsync(club);
                return await SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding club");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Club club)
        {
            try
            {
                _context.Remove(club);
                return await SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting club");
                return false;
            }
        }

        public async Task<IEnumerable<Club>> GetAllAsync()
        {
            try
            {
                return await _context.Clubs
                    .Include(c => c.Address)
                    .Include(c => c.AppUser)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all clubs");
                return Enumerable.Empty<Club>();
            }
        }

        public async Task<Club?> GetByIdAsync(int id, bool trackEntity = true)
        {
            try
            {
                var query = _context.Clubs
                    .Include(c => c.Address)
                    .Include(c => c.AppUser);

                var club = trackEntity
                    ? await query.FirstOrDefaultAsync(c => c.Id == id)
                    : await query.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

                return club;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting club by id: {Id}", id);
                return null;
            }
        }

        public async Task<IEnumerable<Club>> GetSliceAsync(int offset, int size)
        {
            try
            {
                return await _context.Clubs
                    .Include(c => c.Address)
                    .Include(c => c.AppUser)
                    .OrderBy(c => c.Title)
                    .Skip(offset)
                    .Take(size)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slice of clubs");
                return Enumerable.Empty<Club>();
            }
        }

        public async Task<IEnumerable<Club>> GetClubsByCategoryAndSliceAsync(ClubCategory category, int offset, int size)
        {
            try
            {
                return await _context.Clubs
                    .Include(c => c.Address)
                    .Where(c => c.ClubCategory == category)
                    .OrderBy(c => c.Title)
                    .Skip(offset)
                    .Take(size)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clubs by category");
                return Enumerable.Empty<Club>();
            }
        }

        public async Task<int> GetCountByCategoryAsync(ClubCategory category)
        {
            try
            {
                return await _context.Clubs
                    .CountAsync(c => c.ClubCategory == category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count by category");
                return 0;
            }
        }

        public async Task<IEnumerable<Club>> GetClubByCityAsync(string city)
        {
            try
            {
                if (string.IsNullOrEmpty(city))
                    return Enumerable.Empty<Club>();

                return await _context.Clubs
                    .Include(c => c.Address)
                    .Where(c => c.Address != null && c.Address.City != null &&
                               c.Address.City.ToLower().Contains(city.ToLower()))
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clubs by city: {City}", city);
                return Enumerable.Empty<Club>();
            }
        }

        public async Task<IEnumerable<Club>> GetClubsByLocationAsync(string country, string state, string city)
        {
            try
            {
                var clubs = _context.Clubs.Include(c => c.Address).AsQueryable();

                if (!string.IsNullOrEmpty(country))
                {
                    clubs = clubs.Where(c => c.Address != null &&
                                           c.Address.Country != null &&
                                           c.Address.Country.ToLower() == country.ToLower());
                }

                if (!string.IsNullOrEmpty(state))
                {
                    clubs = clubs.Where(c => c.Address != null &&
                                           c.Address.State != null &&
                                           c.Address.State.ToLower() == state.ToLower());
                }

                return await clubs.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clubs by location");
                return Enumerable.Empty<Club>();
            }
        }

        public async Task<bool> UpdateAsync(Club club)
        {
            try
            {
                _context.Update(club);
                return await SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating club");
                return false;
            }
        }

        public async Task<int> GetCountAsync()
        {
            try
            {
                return await _context.Clubs.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting club count");
                return 0;
            }
        }

        public async Task<IEnumerable<Club>> GetClubsByStateAsync(string state)
        {
            try
            {
                if (string.IsNullOrEmpty(state))
                    return Enumerable.Empty<Club>();

                return await _context.Clubs
                    .Include(c => c.Address)
                    .Where(c => c.Address != null && c.Address.State != null &&
                               c.Address.State.ToLower().Contains(state.ToLower()))
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clubs by state: {State}", state);
                return Enumerable.Empty<Club>();
            }
        }

        public async Task<List<State>> GetAllStatesAsync()
        {
            try
            {
                return await _context.States
                    .OrderBy(s => s.StateName)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all states");
                return new List<State>();
            }
        }

        public async Task<List<City>> GetAllCitiesByStateAsync(string state)
        {
            try
            {
                if (string.IsNullOrEmpty(state))
                    return new List<City>();

                return await _context.Cities
                    .Where(c => c.StateCode != null && c.StateCode.Equals(state))
                    .OrderBy(c => c.CityName)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cities by state: {State}", state);
                return new List<City>();
            }
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

        // ✅ FIXED: Added Async suffix and better null handling
        public async Task<IEnumerable<Club>> GetClubsByCountryAsync(string country)
        {
            try
            {
                var countryCode = _countryAliasService.NormalizeCountry(country);

                _logger.LogInformation("Searching clubs for country: {Country} -> {CountryCode}",
                    country, countryCode);

                return await _context.Clubs
                    .Include(c => c.Address)
                    .Where(c => c.Address != null &&
                               c.Address.Country != null &&
                               _countryAliasService.NormalizeCountry(c.Address.Country) == countryCode)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clubs by country: {Country}", country);
                return new List<Club>();
            }
        }


        public async Task<IEnumerable<Club>> GetClubsByProgressiveLocationAsync(string country, string state = null, string city = null)
        {
            try
            {
                var query = _context.Clubs.Include(c => c.Address).AsQueryable();

                // Always filter by country first
                if (!string.IsNullOrEmpty(country))
                {
                    query = query.Where(c => c.Address != null &&
                                            c.Address.Country != null &&
                                            c.Address.Country.ToLower() == country.ToLower());
                }

                var clubs = await query.AsNoTracking().ToListAsync();

                // Priority sorting (not filtering)
                return clubs.OrderBy(c =>
                {
                    if (!string.IsNullOrEmpty(city) &&
                        c.Address?.City?.ToLower() == city?.ToLower())
                        return 1; // Same city - highest priority
                    else if (!string.IsNullOrEmpty(state) &&
                             c.Address?.State?.ToLower() == state?.ToLower())
                        return 2; // Same state - medium priority
                    else
                        return 3; // Same country - lowest priority
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in progressive location search");
                return Enumerable.Empty<Club>();
            }
        }
    }
}