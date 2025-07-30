using Domain.Dtos.Auth;
using Services.Abstractions.IServices;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Services.Auth
{
    public class LocationService : ILocationService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private const string CACHE_KEY_GOVERNORATES = "egypt_governorates";
        private const string CACHE_KEY_CITIES = "egypt_cities_";
        private const string CACHE_KEY_ZONES = "egypt_zones_";
        private readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(24); // Cache for 24 hours

        public LocationService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<List<LocationDto>> GetAllGovernoratesAsync()
        {
            // Check cache first
            if (_cache.TryGetValue(CACHE_KEY_GOVERNORATES, out List<LocationDto> cachedGovernorates))
            {
                return cachedGovernorates;
            }

            try
            {
                // Fetch from external API (TechLabs or similar)
                var response = await _httpClient.GetStringAsync("https://api.example.com/egypt/governorates");
                var governorates = JsonSerializer.Deserialize<List<LocationDto>>(response);

                // Cache the result
                _cache.Set(CACHE_KEY_GOVERNORATES, governorates, CACHE_DURATION);

                return governorates;
            }
            catch (Exception)
            {
                // Fallback to hardcoded Egyptian governorates if API fails
                return GetFallbackGovernorates();
            }
        }

        public async Task<List<LocationDto>> GetCitiesByGovernorateAsync(int governorateId)
        {
            var cacheKey = $"{CACHE_KEY_CITIES}{governorateId}";

            if (_cache.TryGetValue(cacheKey, out List<LocationDto> cachedCities))
            {
                return cachedCities;
            }

            try
            {
                var response = await _httpClient.GetStringAsync($"https://api.example.com/egypt/cities/{governorateId}");
                var cities = JsonSerializer.Deserialize<List<LocationDto>>(response);

                _cache.Set(cacheKey, cities, CACHE_DURATION);

                return cities;
            }
            catch (Exception)
            {
                return GetFallbackCities(governorateId);
            }
        }

        public async Task<List<LocationDto>> GetZonesByCityAsync(int cityId)
        {
            var cacheKey = $"{CACHE_KEY_ZONES}{cityId}";

            if (_cache.TryGetValue(cacheKey, out List<LocationDto> cachedZones))
            {
                return cachedZones;
            }

            try
            {
                var response = await _httpClient.GetStringAsync($"https://api.example.com/egypt/zones/{cityId}");
                var zones = JsonSerializer.Deserialize<List<LocationDto>>(response);

                _cache.Set(cacheKey, zones, CACHE_DURATION);

                return zones;
            }
            catch (Exception)
            {
                return GetFallbackZones(cityId);
            }
        }

        public async Task<LocationHierarchyDto> GetFullHierarchyAsync()
        {
            // This method could be used to get the complete hierarchy at once
            // Useful for frontend caching
            var governorates = await GetAllGovernoratesAsync();
            var hierarchy = new LocationHierarchyDto
            {
                Governorates = new List<GovernorateDto>()
            };

            foreach (var gov in governorates)
            {
                var cities = await GetCitiesByGovernorateAsync(gov.Id);
                var governorateDto = new GovernorateDto
                {
                    Id = gov.Id,
                    Name = gov.Name,
                    NameAr = gov.NameAr,
                    Cities = new List<CityDto>()
                };

                foreach (var city in cities)
                {
                    var zones = await GetZonesByCityAsync(city.Id);
                    governorateDto.Cities.Add(new CityDto
                    {
                        Id = city.Id,
                        Name = city.Name,
                        NameAr = city.NameAr,
                        GovernorateId = gov.Id,
                        Zones = zones.Select(z => new ZoneDto
                        {
                            Id = z.Id,
                            Name = z.Name,
                            NameAr = z.NameAr,
                            CityId = city.Id
                        }).ToList()
                    });
                }

                hierarchy.Governorates.Add(governorateDto);
            }

            return hierarchy;
        }

        // Fallback methods with hardcoded Egyptian data
        private List<LocationDto> GetFallbackGovernorates()
        {
            return new List<LocationDto>
            {
                new LocationDto { Id = 1, Name = "Cairo", NameAr = "القاهرة" },
                new LocationDto { Id = 2, Name = "Alexandria", NameAr = "الإسكندرية" },
                new LocationDto { Id = 3, Name = "Giza", NameAr = "الجيزة" },
                new LocationDto { Id = 4, Name = "Qalyubia", NameAr = "القليوبية" },
                new LocationDto { Id = 5, Name = "Dakahlia", NameAr = "الدقهلية" },
                // Add more Egyptian governorates...
            };
        }

        private List<LocationDto> GetFallbackCities(int governorateId)
        {
            // Return cities based on governorate ID
            return governorateId switch
            {
                1 => new List<LocationDto> // Cairo
                {
                    new LocationDto { Id = 101, Name = "Nasr City", NameAr = "مدينة نصر", ParentId = 1 },
                    new LocationDto { Id = 102, Name = "Heliopolis", NameAr = "مصر الجديدة", ParentId = 1 },
                    new LocationDto { Id = 103, Name = "Maadi", NameAr = "المعادي", ParentId = 1 },
                },
                3 => new List<LocationDto> // Giza
                {
                    new LocationDto { Id = 301, Name = "6th of October", NameAr = "6 أكتوبر", ParentId = 3 },
                    new LocationDto { Id = 302, Name = "Sheikh Zayed", NameAr = "الشيخ زايد", ParentId = 3 },
                },
                _ => new List<LocationDto>()
            };
        }

        private List<LocationDto> GetFallbackZones(int cityId)
        {
            // Return zones based on city ID
            return cityId switch
            {
                101 => new List<LocationDto> // Nasr City zones
                {
                    new LocationDto { Id = 10101, Name = "First District", NameAr = "الحي الأول", ParentId = 101 },
                    new LocationDto { Id = 10102, Name = "Seventh District", NameAr = "الحي السابع", ParentId = 101 },
                },
                301 => new List<LocationDto> // 6th of October zones
                {
                    new LocationDto { Id = 30101, Name = "District 1", NameAr = "الحي الأول", ParentId = 301 },
                    new LocationDto { Id = 30102, Name = "District 2", NameAr = "الحي الثاني", ParentId = 301 },
                },
                _ => new List<LocationDto>()
            };
        }
    }
}