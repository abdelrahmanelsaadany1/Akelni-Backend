using AutoMapper;
using Domain.Dtos.CategoryDto;
using Domain.Dtos.ItemDto;
using Domain.Entities;

namespace Persistence.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CategoryCreateDto, Category>().ReverseMap();
            CreateMap<ItemDto, Item>().ReverseMap();
        }
    }
}