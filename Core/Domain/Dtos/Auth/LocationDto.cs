namespace Domain.Dtos.Auth
{
    public class LocationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; } // Arabic name for Egyptian locations
        public int? ParentId { get; set; } // For hierarchical structure
    }

    public class LocationHierarchyDto
    {
        public List<GovernorateDto> Governorates { get; set; }
    }

    public class GovernorateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public List<CityDto> Cities { get; set; }
    }

    public class CityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public int GovernorateId { get; set; }
        public List<ZoneDto> Zones { get; set; }
    }

    public class ZoneDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public int CityId { get; set; }
    }
}