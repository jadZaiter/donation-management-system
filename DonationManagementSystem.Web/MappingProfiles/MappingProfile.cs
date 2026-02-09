using AutoMapper;
using DonationManagementSystem.Application.DonationCases.Dtos;
using DonationManagementSystem.Domain.Entities;

namespace DonationManagementSystem.Web.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category mapping
            CreateMap<Category, CategoryDto>().ReverseMap();

            // Tag mapping
            CreateMap<Tag, TagDto>().ReverseMap();

            // DonationCase mapping
            CreateMap<DonationCase, DonationCaseCardDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.DonationCaseTags.Select(dct => dct.Tag).ToList()))
                .ReverseMap();
        }
    }
}