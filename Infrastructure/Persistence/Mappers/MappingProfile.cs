using AutoMapper;
using Domain.Dtos.AddOnDto;
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
            CreateMap<AddOnDto, AddOn>().ReverseMap();

            CreateMap<ItemCreateUpdateDto, Item>()
                .ForMember(dest => dest.ItemAddOns, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCombos, opt => opt.Ignore());


            CreateMap<Item, ItemClassDto>()
                .ForMember(dest => dest.AddOnIds, opt => opt.MapFrom(src => src.ItemAddOns.Select(ia => ia.AddOnId)))
                .ForMember(dest => dest.ComboIds, opt => opt.MapFrom(src => src.ItemCombos.Select(ic => ic.ComboId)))
                .ReverseMap();
        }
    }
}