using AutoMapper;
using Domain.Dtos.AddOnDto;
using Domain.Dtos.CategoryDto;
using Domain.Dtos.ComboDto;
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
                .ForMember(dest => dest.ItemCombos, opt => opt.Ignore())
                .ForMember(dest => dest.SizePricing, opt => opt.Ignore());

            CreateMap<ItemSizePrice, ItemSizePriceDto>().ReverseMap();

            CreateMap<Item, ItemClassDto>()
                .ForMember(dest => dest.AddOnIds, opt => opt.MapFrom(src => src.ItemAddOns.Select(ia => ia.AddOnId)))
                .ForMember(dest => dest.ComboIds, opt => opt.MapFrom(src => src.ItemCombos.Select(ic => ic.ComboId)))
                .ForMember(dest => dest.SizePricing, opt => opt.MapFrom(src => src.SizePricing))
                .ForMember(dest => dest.AddOns, opt => opt.MapFrom(src => src.ItemAddOns.Select(ia => new AddOnGetDto
                {
                    Id = ia.AddOn.Id,
                    Name = ia.AddOn.Name,
                    AdditionalPrice = ia.AdditionalPrice,
                    ImageUrl = ia.ImageUrl
                })))
                .ForMember(dest => dest.Combos, opt => opt.MapFrom(src => src.ItemCombos.Select(ic => new ComboGetDto
                {
                    Id = ic.Combo.Id,
                    Name = ic.Combo.Name,
                    ComboPrice = ic.ComboPrice,
                    ImageUrl = ic.ImageUrl
                })))
                .ReverseMap();
        }
    }
}