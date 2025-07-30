using Domain.Dtos.Auth;

namespace Services.Abstractions.IServices
{
    public interface ILocationService
    {
        Task<List<LocationDto>> GetAllGovernoratesAsync();
        Task<List<LocationDto>> GetCitiesByGovernorateAsync(int governorateId);
        Task<List<LocationDto>> GetZonesByCityAsync(int cityId);
        Task<LocationHierarchyDto> GetFullHierarchyAsync();
    }
}