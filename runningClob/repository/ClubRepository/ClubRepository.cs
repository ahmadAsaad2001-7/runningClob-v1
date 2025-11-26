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
                _logger.LogInformation("🔍 REPOSITORY: Starting search for: '{Country}'", country);

                if (string.IsNullOrWhiteSpace(country))
                {
                    _logger.LogWarning("🔍 REPOSITORY: Country parameter is empty");
                    return new List<Club>();
                }

                // STEP 1: Normalize the search country
                var searchCountryCode = _countryAliasService.NormalizeCountry(country);
                _logger.LogInformation("🔍 REPOSITORY: Normalized search country: '{SearchCountryCode}'", searchCountryCode);

                // STEP 2: Get ALL clubs with addresses (no filtering yet)
                var allClubs = await _context.Clubs
                    .Include(c => c.Address)
                    .Where(c => c.Address != null) // Only clubs with addresses
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("🔍 REPOSITORY: Retrieved {Count} clubs with addresses", allClubs.Count);

                // STEP 3: Filter in memory (more reliable than complex LINQ)
                var filteredClubs = allClubs
                    .Where(c =>
                    {
                        if (string.IsNullOrEmpty(c.Address.Country))
                            return false;

                        // Normalize each club's country and compare
                        var clubCountryCode = _countryAliasService.NormalizeCountry(c.Address.Country);
                        return clubCountryCode == searchCountryCode;
                    })
                    .ToList();

                _logger.LogInformation("🔍 REPOSITORY: Found {FilteredCount} clubs matching '{SearchCountryCode}'",
                    filteredClubs.Count, searchCountryCode);

                // STEP 4: Debug logging - show what we found
                if (filteredClubs.Any())
                {
                    _logger.LogInformation("🔍 REPOSITORY: Matching clubs:");
                    foreach (var club in filteredClubs)
                    {
                        var clubCountryCode = _countryAliasService.NormalizeCountry(club.Address.Country);
                        _logger.LogInformation("   ✅ ID: {Id}, Title: {Title}, Country: '{Country}' -> '{Code}'",
                            club.Id, club.Title, club.Address.Country, clubCountryCode);
                    }
                }
                else
                {
                    _logger.LogWarning("🔍 REPOSITORY: NO CLUBS FOUND for '{SearchCountryCode}'", searchCountryCode);

                    // Log what countries we actually have for debugging
                    var availableCountries = allClubs
                        .Where(c => !string.IsNullOrEmpty(c.Address.Country))
                        .Select(c => new {
                            Original = c.Address.Country,
                            Normalized = _countryAliasService.NormalizeCountry(c.Address.Country)
                        })
                        .Distinct()
                        .ToList();

                    _logger.LogInformation("🔍 REPOSITORY: Available countries in database:");
                    foreach (var countryInfo in availableCountries)
                    {
                        _logger.LogInformation("   - '{Original}' -> '{Normalized}'",
                            countryInfo.Original, countryInfo.Normalized);
                    }
                }

                return filteredClubs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ REPOSITORY: Error in GetClubsByCountryAsync for '{Country}'", country);
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